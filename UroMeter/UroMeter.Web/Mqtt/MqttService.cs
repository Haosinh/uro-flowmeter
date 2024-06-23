using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;

namespace UroMeter.Web.Mqtt;

/// <summary>
/// Mqtt service.
/// </summary>
public class MqttService
{
    private string broker = string.Empty;
    private string username = string.Empty;
    private string password = string.Empty;

    private readonly ILogger<MqttService> logger;
    private Dictionary<string, IMqttClient> clients = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    public MqttService(ILogger<MqttService> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="broker"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task Setup(string broker, string username, string password)
    {
        this.broker = broker;
        this.username = username;
        this.password = password;

        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithClientId("register")
            .WithCredentials(username, password)
            .WithTcpServer(broker, 8883)
            .WithTlsOptions(o => o.UseTls())
            .Build();

        client.ApplicationMessageReceivedAsync += e =>
        {
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            logger.LogInformation("Received application message {message}", message);

            return Task.CompletedTask;
        };

        await client.ConnectAsync(options);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("backend")
            .WithPayload("Hello")
            .Build();

        await client.PublishAsync(message, CancellationToken.None);

        var subscriberOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter("register")
            .Build();

        var response = await client.SubscribeAsync(subscriberOptions);

        logger.LogInformation("MQTT client subscribed to topic");
        logger.LogInformation(JsonSerializer.Serialize(response));

        clients.TryAdd("register", client);
    }
}
