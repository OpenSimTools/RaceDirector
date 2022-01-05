using Microsoft.Extensions.DependencyInjection;
using RaceDirector.DependencyInjection;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using System;

namespace RaceDirector.Plugin
{
    public class DefaultPlugin : IPlugin
    {
        public void Init(IServiceCollection services)
        {
            services
                .AddSingletonWithInterfaces(_ => new Pipeline.Games.R3E.Game.Config(TimeSpan.FromMilliseconds(15)))
                .AddSingletonWithInterfaces<Pipeline.Games.R3E.Game>()
                .AddSingletonWithInterfaces(_ => new ProcessMonitorNode.Config(TimeSpan.FromSeconds(5)))
                .AddTransientWithInterfaces<ProcessMonitorNode>()
                .AddTransientWithInterfaces<TelemetryReaderNode>();
        }
    }
}
