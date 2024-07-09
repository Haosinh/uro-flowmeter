using System.Globalization;
using System.Text;
using CsvHelper;
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

                    var newRecord = new Record { PatientId = device.PatientId.Value };
                    await dbContext.AddAsync(newRecord);

                    await dbContext.SaveChangesAsync();

                    using var reader = new StringReader(content);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                    var dataRecordDtos = csv.GetRecords<DataRecordDto>().ToList();
                    var records = dataRecordDtos.Select(e => new RecordData
                    {
                        Time = e.Time,
                        Volume = e.Volume,
                        RecordId = newRecord.Id
                    }).ToList();
                    await dbContext.RecordDatas.AddRangeAsync(records);

                    await dbContext.SaveChangesAsync();
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
