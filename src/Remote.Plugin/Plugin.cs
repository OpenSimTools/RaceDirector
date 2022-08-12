using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Remote.Pipeline;
using RaceDirector.Plugin;

namespace RaceDirector.Remote;

public class Plugin : IPlugin
{

    public void Init(IConfiguration configuration, IServiceCollection services)
    {
        services
            .AddTransientWithInterfaces<WebSocketTelemetryNode>();
    }
}