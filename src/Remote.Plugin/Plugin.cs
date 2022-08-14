using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Remote.Pipeline;
using RaceDirector.Plugin;

namespace RaceDirector.Remote;

public class Plugin : PluginBase<PluginBase.Config>
{
    protected override void Init(PluginBase.Config _, IServiceCollection services)
    {
        services
            .AddTransientWithInterfaces<WsTelemetryNode>();
    }
}