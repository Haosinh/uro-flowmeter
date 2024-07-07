using MQTTnet.Client;
using UroMeter.Web.Mqtt;
using UroMeter.Web.Settings;

namespace UroMeter.Web.Extensions;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services)
    {
        services.AddMqttClientServiceWithConfig(options =>
        {
            var clientSettings = AppSettingsProvider.ClientSettings;
            var brokerHostSettings = AppSettingsProvider.BrokerHostSettings;

            options
                .WithClientId(clientSettings.Id)
                .WithCredentials(clientSettings.UserName, clientSettings.Password)
                .WithTcpServer(brokerHostSettings.Host, brokerHostSettings.Port)
                .WithTlsOptions(o => o.UseTls());
        });
        return services;
    }

    private static IServiceCollection AddMqttClientServiceWithConfig(this IServiceCollection services, Action<MqttClientOptionsBuilder> configure)
    {
        services.AddSingleton<MqttClientOptions>(_ =>
        {
            var optionBuilder = new MqttClientOptionsBuilder();
            configure(optionBuilder);

            return optionBuilder.Build();
        });
        services.AddSingleton<MqttClientService>();
        services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetRequiredService<MqttClientService>());
        services.AddSingleton<MqttClientServiceProvider>(serviceProvider =>
        {
            var mqttClientService = serviceProvider.GetRequiredService<MqttClientService>();
            var mqttClientServiceProvider = new MqttClientServiceProvider(mqttClientService);

            return mqttClientServiceProvider;
        });

        return services;
    }
}
