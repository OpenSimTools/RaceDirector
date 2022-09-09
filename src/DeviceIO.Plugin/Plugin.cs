using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Plugin;

namespace RaceDirector.DeviceIO;

public class Plugin : PluginBase<Plugin.Configuration>
{
    public class Configuration : PluginBase.Config, DeviceIoNode.IConfiguration
    {
        public Dictionary<string, string> KeyMappings { get; } = new();
        public TimeSpan WaitBetweenKeys { get; } = TimeSpan.Zero;
    }

    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        services
            .AddTransient<DeviceIoNode.IConfiguration>(_ => configuration)
            .AddSingletonWithInterfaces<DeviceIoNode>();
    }
}