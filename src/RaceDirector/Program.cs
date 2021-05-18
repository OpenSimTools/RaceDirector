using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDirector.DependencyInjection;
using RaceDirector.Pipeline;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using System;
using System.Runtime.Versioning;

namespace RaceDirector
{
    [SupportedOSPlatform("windows")]
    static class Program
    {
        static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    Init(services);
                    new Plugin.HUD.Plugin().Init(services);
                }).Build();

            Console.WriteLine("Starting pipeline");

            var nodes = host.Services.GetServices<INode>();
            PipelineBuilder.LinkNodes(nodes);

            host.WaitForShutdown();
        }

        static void Init(IServiceCollection services)
        {
            services
                .AddSingletonWithInterfaces(_ => new Pipeline.Games.R3E.Game.Config(TimeSpan.FromMilliseconds(500)))
                .AddSingletonWithInterfaces<Pipeline.Games.R3E.Game>()
                .AddSingletonWithInterfaces(_ => new ProcessMonitorNode.Config(TimeSpan.FromSeconds(5)))
                .AddTransientWithInterfaces<ProcessMonitorNode>()
                .AddTransientWithInterfaces<TelemetryReaderNode>()
                .AddTransientWithInterfaces<TelemetryLoggerNode>();
        }
    }
}
