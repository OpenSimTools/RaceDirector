using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using System;
using System.Runtime.Versioning;

namespace RaceDirector.Plugin
{
    [SupportedOSPlatform("windows")]
    public class DefaultPlugin : IPlugin
    {
        [SupportedOSPlatform("windows")]
        public void Init(IServiceCollection services)
        {
            services
                .AddSingletonWithInterfaces(_ => new Pipeline.Games.R3E.Game.Config(TimeSpan.FromMilliseconds(15)))
                .AddSingletonWithInterfaces<Pipeline.Games.R3E.Game>()
                .AddSingletonWithInterfaces(_ => new Pipeline.Games.ACC.Game.Config(TimeSpan.FromMilliseconds(15)))
                .AddSingletonWithInterfaces<Pipeline.Games.ACC.Game>()
                .AddSingletonWithInterfaces(_ => new ProcessMonitorNode.Config(TimeSpan.FromSeconds(5)))
                .AddTransientWithInterfaces<ProcessMonitorNode>()
                .AddTransientWithInterfaces<TelemetryReaderNode>();
        }
    }
}
