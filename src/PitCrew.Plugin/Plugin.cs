using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Plugin;

namespace RaceDirector.PitCrew;

public class Plugin : PluginBase<Plugin.Configuration>
{
    public class Configuration
    {
        public string? ServerUrl { get; set; } = null;
    }

    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        if (configuration.ServerUrl is not null)
            services.AddTransientWithInterfaces(_ => new PitCrewClient(configuration.ServerUrl));
    }
}