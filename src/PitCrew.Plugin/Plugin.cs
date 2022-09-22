using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.PitCrew.Pipeline;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.Plugin;

namespace RaceDirector.PitCrew;


public class Plugin : PluginBase<Plugin.Configuration>
{
    public class Configuration : PluginBase.Config
    {
        public string ServerUrl { get; set; } = null!;
        public TimeSpan MaxMenuNavigationWait { get; set; } = TimeSpan.Zero;
    }

    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        services
            .AddSingletonWithInterfaces(_ => new PitCrewClient(configuration.ServerUrl))
            .AddSingletonWithInterfaces(_ => new ACCPitMenuNavigator(configuration.MaxMenuNavigationWait))
            .AddSingletonWithInterfaces<R3EPitMenuNavigator>()
            .AddSingletonWithInterfaces<PitCrewNode>()
            .AddSingletonWithInterfaces<PitMenuNode>();
    }
}