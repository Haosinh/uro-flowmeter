using System.Text;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using UroMeter.DataAccess;
using UroMeter.DataAccess.Models;
using UroMeter.Web.Extensions;
using UroMeter.Web.Mqtt.Dtos;

namespace UroMeter.Web.Mqtt;

/// <summary>
/// Mqtt service.
/// </summary>
public partial class MqttClientService : IMqttClientService
{
    private const string RegisterTopic = "register";
    private const string DataTopic = "data/#";
    private const string DataPrefixTopic = "data";

    private readonly ILogger<MqttClientService> logger;
    private readonly IMqttClient mqttClient;
    private readonly MqttClientOptions options;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Constructor.
    /// </summary>
    public MqttClientService(MqttClientOptions options, ILogger<MqttClientService> logger, IServiceProvider serviceProvider)
    {
        this.options = options;
        this.logger = logger;
        this.serviceProvider = serviceProvider;

        mqttClient = new MqttFactory().CreateMqttClient();

        mqttClient.ConnectedAsync += HandleConnectedAsync;
        mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var content = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

        // logger.LogDebug("{handler}: Receive message with Topic:{topic}, Content:{content}", nameof(MqttClientService), topic, content);

        switch (topic)
        {
            case RegisterTopic:
                {
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var tokens = content.Split("|");
                    var dto = tokens.MapToRegisterRecordDto();

                    var device = await dbContext.Devices.FirstOrDefaultAsync(e => e.MacAddress == dto.MacAddress);
                    if (device is null)
                    {
                        device = new Device { MacAddress = dto.MacAddress };

                        await dbContext.Devices.AddAsync(device);
                    }
                    else
                    {
                        device.LastSeen = DateTimeOffset.Now;
                    }

                    await dbContext.SaveChangesAsync();
                }

                break;
            case not null when topic.StartsWith(DataPrefixTopic):
                {
                    var topicTokens = topic.Split("/");
                    if (topicTokens.Length < 2)
                    {
                        logger.LogWarning("{handler}: Receive invalid topic with Topic:{topic}, Content:{content}", nameof(MqttClientService), topic, content);
                        return;
                    }

                    var macAddress = topicTokens[1];

                    var dataTokens = content.Split(",");
                    if (dataTokens.Length < 1)
                    {
                        logger.LogWarning("{handler}: Receive invalid data with Topic:{topic}, Content:{content}", nameof(MqttClientService), topic, content);
                        return;
                    }

                    await using var scope = serviceProvider.CreateAsyncScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var device = dbContext.Devices.FirstOrDefault(x => x.MacAddress == macAddress);
                    if (device is null)
                    {
                        logger.LogWarning("{handler}: Cannot find device with MacAddress:{macAddress}, Topic:{topic}, Content:{content}", nameof(MqttClientService), macAddress, topic, content);
                        return;
                    }

                    if (device.PatientId is null)
                    {
                        logger.LogWarning("{handler}: Cannot find patient with Device:{device}, LastSeen{lastSeen}, Topic:{topic}, Content:{content}", nameof(MqttClientService), device.MacAddress, device.LastSeen, topic, content);
                        return;
                    }

                    switch (dataTokens.GetCommand())
                    {
                        case DataCommand.BEGIN_RECORD:
                            {
                                var beginRecordDto = dataTokens.MapToBeginDataRecordDto();

                                var latestRecord = await dbContext.Records
                                    .OrderByDescending(e => e.CheckUpAt)
                                    .FirstOrDefaultAsync(e => e.PatientId == device.PatientId);

                                if (latestRecord is not null && !latestRecord.Finished)
                                {
                                    logger.LogWarning("{handler}: The latest record is not finished, cannot create new record with RecordId:{recordId}, CheckUpAt:{checkUpAt}, PatientId:{patientId}, Device:{macAddress}, LastSeen{lastSeen}, Topic:{topic}, Content:{content}", nameof(MqttClientService), latestRecord.Id, latestRecord.CheckUpAt, device.PatientId, device.MacAddress, device.LastSeen, topic, content);
                                    return;
                                }

                                var newRecord = new Record
                                {
                                    PatientId = device.PatientId.Value,
                                    RecordAt = beginRecordDto.RecordAt.LocalDateTime
                                };
                                await dbContext.AddAsync(newRecord);

                                await dbContext.SaveChangesAsync();
                            }

                            break;
                        case DataCommand.END_RECORD:
                            {
                                var latestRecord = await dbContext.Records
                                     .OrderByDescending(e => e.CheckUpAt)
                                     .FirstOrDefaultAsync(e => e.PatientId == device.PatientId);

                                if (latestRecord is null)
                                {
                                    logger.LogWarning("{handler}: The latest record is not found, cannot closed record with PatientId:{patientId}, Device:{device}, LastSeen{lastSeen}, Topic:{topic}, Content:{content}", nameof(MqttClientService), device.PatientId, device.MacAddress, device.LastSeen, topic, content);
                                    return;
                                }

                                if (latestRecord.Finished)
                                {
                                    logger.LogWarning("{handler}: The latest record is finished, cannot closed record with with PatientId:{patientId}, RecordId:{recordId}, CheckUpAt:{checkUpAt}, Device:{device}, LastSeen{lastSeen}, Topic:{topic}, Content:{content}", nameof(MqttClientService), device.PatientId, latestRecord.Id, latestRecord.CheckUpAt, device.MacAddress, device.LastSeen, topic, content);
                                    return;
                                }

                                latestRecord.Finished = true;

                                await dbContext.SaveChangesAsync();
                            }

                            break;
                        case DataCommand.RECORD:
                            {
                                var latestRecord = await dbContext.Records
                                    .OrderByDescending(e => e.CheckUpAt)
                                    .FirstOrDefaultAsync(e => e.PatientId == device.PatientId);

                                if (latestRecord is null)
                                {
                                    logger.LogWarning("{handler}: The latest record is not found, cannot insert data to record with PatientId:{patientId}, Device:{device}, LastSeen{lastSeen}, Topic:{topic}, Content:{content}", nameof(MqttClientService), device.PatientId, device.MacAddress, device.LastSeen, topic, content);
                                    return;
                                }

                                if (latestRecord.Finished)
                                {
                                    logger.LogWarning("{handler}: The latest record is finished, cannot insert data to record with with PatientId:{patientId}, RecordId:{recordId}, CheckUpAt:{checkUpAt}, Device:{device}, LastSeen{lastSeen}, Topic:{topic}, Content:{content}", nameof(MqttClientService), device.PatientId, latestRecord.Id, latestRecord.CheckUpAt, device.MacAddress, device.LastSeen, topic, content);
                                    return;
                                }

                                var dataRecordDto = dataTokens.MapToDataRecordDto();
                                if (!dataRecordDto.IsValid())
                                {
                                    logger.LogWarning("{handler}: Receive invalid data with Topic:{topic}, Content:{content}", nameof(MqttClientService), topic, content);
                                    return;
                                }

                                var recordData = new RecordData
                                {
                                    RecordAt = latestRecord.RecordAt.AddMilliseconds(dataRecordDto.Time),
                                    Volume = dataRecordDto.Volume!.Value,
                                    RecordId = latestRecord.Id
                                };
                                await dbContext.RecordDatas.AddAsync(recordData);

                                await dbContext.SaveChangesAsync();
                            }

                            break;
                    }
                }

                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        logger.LogInformation("HandleDisconnectedAsync");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public async Task HandleConnectedAsync(MqttClientConnectedEventArgs e)
    {
        logger.LogInformation("connected");
        await mqttClient.SubscribeAsync(RegisterTopic);
        await mqttClient.SubscribeAsync(DataTopic);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await mqttClient.ConnectAsync(options, cancellationToken);

        // This sample shows how to reconnect when the connection was dropped.
        // This approach uses a custom Task/Thread which will monitor the connection status.
        // This is the recommended way but requires more custom code!
        // https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Connection_Samples.cs
        _ = Task.Run(async () =>
        {
            // User proper cancellation and no while(true).
            while (true)
            {
                try
                {
                    // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                    if (!await mqttClient.TryPingAsync(cancellationToken))
                    {
                        await mqttClient.ConnectAsync(options, cancellationToken);

                        // Subscribe to topics when session is clean etc.
                        logger.LogInformation("The MQTT client is connected.");
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception properly (logging etc.).
                    logger.LogError(ex, "The MQTT client connection failed");
                }
                finally
                {
                    // Check the connection state every 5 seconds and perform a reconnect if required.
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var disconnectOption = new MqttClientDisconnectOptions
            {
                Reason = MqttClientDisconnectOptionsReason.NormalDisconnection,
                ReasonString = "NormalDisconnection"
            };
            await mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
        }

        await mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
    }
}
