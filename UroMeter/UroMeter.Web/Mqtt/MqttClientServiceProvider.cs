namespace UroMeter.Web.Mqtt;

/// <summary>
/// 
/// </summary>
public class MqttClientServiceProvider
{
    private readonly IMqttClientService mqttClientService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mqttClientService"></param>
    public MqttClientServiceProvider(IMqttClientService mqttClientService)
    {
        this.mqttClientService = mqttClientService;
    }
}
