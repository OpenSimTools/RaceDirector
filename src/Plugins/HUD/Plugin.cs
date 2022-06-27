using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Plugin.HUD.Pipeline;

namespace RaceDirector.Plugin.HUD
{
    public class Plugin : PluginBase<Plugin.Configuration>
    {
        protected override void Init(Configuration configuration, IServiceCollection services)
        {
            services
                .AddSingletonWithInterfaces(_ => configuration.DashboardServer)
                .AddTransientWithInterfaces<DashboardServer>()
                .AddTransientWithInterfaces<WebSocketTelemetryNode>();
        }

        public class Configuration
        {
            public DashboardServer.Config DashboardServer { get; set; } = null!;
        }
    }
}
