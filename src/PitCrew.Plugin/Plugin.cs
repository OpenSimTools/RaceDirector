using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.PitCrew.Pipeline;
using RaceDirector.Plugin;

namespace RaceDirector.PitCrew;


public class Plugin : PluginBase<Plugin.Configuration>
{
    public class Configuration : PluginBase.Config
    {
        public string ServerUrl { get; set; } = null!;
    }

    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        services
            .AddSingletonWithInterfaces(_ => new PitCrewClient(configuration.ServerUrl))
            .AddSingletonWithInterfaces<PitCrewNode>()
            .AddSingletonWithInterfaces<PitMenuNode>();
    }
}