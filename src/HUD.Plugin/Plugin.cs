using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.HUD.Pipeline;
using RaceDirector.Plugin;

namespace RaceDirector.HUD;

public class Plugin : PluginBase<Plugin.Configuration>
{
    public class Configuration : PluginBase.Config
    {
        public DashboardServer.Config DashboardServer { get; set; } = null!;
    }

    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        services
            .AddSingletonWithInterfaces(_ => configuration.DashboardServer)
            .AddTransientWithInterfaces<DashboardServer>();
    }
}