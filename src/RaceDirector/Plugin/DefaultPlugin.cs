using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using System.Runtime.Versioning;

namespace RaceDirector.Plugin;

[SupportedOSPlatform("windows")]
public class DefaultPlugin : PluginBase<DefaultPlugin.Configuration>
{
    [SupportedOSPlatform("windows")]
    protected override void Init(Configuration configuration, IServiceCollection services)
    {
        services
            .AddSingletonWithInterfaces<Pipeline.Games.R3E.Game>()
            .AddSingletonWithInterfaces(_ => configuration.Games.R3E)
            .AddSingletonWithInterfaces<Pipeline.Games.ACC.Game>()
            .AddSingletonWithInterfaces(_ => configuration.Games.Acc)
            .AddTransientWithInterfaces<ProcessMonitorNode>()
            .AddSingletonWithInterfaces(_ => configuration.ProcessMonitor)
            .AddTransientWithInterfaces<TelemetryReaderNode>();
    }

    public class Configuration
    {
        public ProcessMonitorNode.Config ProcessMonitor { get; set; } = null!;
        public GamesConfiguration Games { get; set; } = null!;
    }

    public class GamesConfiguration
    {
        public Pipeline.Games.ACC.Game.Config Acc { get; set; } = null!;
        public Pipeline.Games.R3E.Game.Config R3E { get; set; } = null!;
    }
}