using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin;

public static class PluginLoader
{
    public static void InitPlugins(IConfiguration config, IServiceCollection services)
    {
        foreach (var p in InstantiatePlugins())
            p.Init(config, services);
    }
    
    private static IEnumerable<IPlugin> InstantiatePlugins()
    {
        // Until dynamic loading is implemented
        return new IPlugin[]
        {
            new DefaultPlugin(),
            new RaceDirector.Remote.Plugin(),
            new RaceDirector.HUD.Plugin(),
            new RaceDirector.PitCrew.Plugin(),
            new RaceDirector.DeviceIO.Plugin()
        };
    }
}