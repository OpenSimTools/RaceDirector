using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Plugin.HUD.Pipeline;
using System.Net;

namespace RaceDirector.Plugin.HUD
{
    public class Plugin : IPlugin
    {
        public void Init(IServiceCollection services)
        {
            services
                .AddSingletonWithInterfaces(_ => new DashboardServer.Config(IPAddress.Any))
                .AddTransientWithInterfaces<DashboardServer>()
                .AddTransientWithInterfaces<WebSocketTelemetryNode>();
        }
    }
}
