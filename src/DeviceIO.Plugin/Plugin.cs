using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Plugin;

namespace RaceDirector.DeviceIO;

public class Plugin : PluginBase<Plugin.Configuration>
{
    public class Configuration : PluginBase.Config, DeviceIoNode.Configuration
    {
        public Dictionary<string, string> KeyMappings { get; } = new ();
    }

    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        services
            .AddTransient<DeviceIoNode.Configuration>(_ => configuration)
            .AddSingletonWithInterfaces<DeviceIoNode>();
    }
}