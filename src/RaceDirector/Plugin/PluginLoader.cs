using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin;

[SupportedOSPlatform("windows")]
public static class PluginLoader
{
    public static void InitPlugins(IConfiguration config, IServiceCollection services)
    {
        foreach (var p in InstantiatePlugins())
            p.Init(config, services);
    }
    
    [SupportedOSPlatform("windows")]
    private static IEnumerable<IPlugin> InstantiatePlugins()
    {
        // Until dynamic loading is implemented
        return new IPlugin[]
        {
            new DefaultPlugin(),
            new RaceDirector.Remote.Plugin(),
            new RaceDirector.HUD.Plugin(),
            new RaceDirector.PitCrew.Plugin()
        };
    }
}