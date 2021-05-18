using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDirector.DependencyInjection;
using RaceDirector.Pipeline;
using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.GameMonitor.Config;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Plugin.HUD.Pipeline;
using RaceDirector.Plugin.HUD.Pipeline.Config;
using System;
using System.Net;
using System.Runtime.Versioning;

namespace RaceDirector
{
    [SupportedOSPlatform("windows")]
    static class Program
    {
        static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) => services
                    .AddSingletonWithInterfaces(_ => new Pipeline.Games.R3E.Game.Config(TimeSpan.FromMilliseconds(500))) // FIXME
                    .AddSingletonWithInterfaces<Pipeline.Games.R3E.Game>()
                    .AddSingletonWithInterfaces(_ => new ProcessMonitorNodeConfig(TimeSpan.FromSeconds(5)))
                    .AddTransientWithInterfaces<ProcessMonitorNode>()
                    .AddTransientWithInterfaces<TelemetryReaderNode>()
                    .AddTransientWithInterfaces<TelemetryLoggerNode>()
                    .AddSingletonWithInterfaces(_ => new DashboardServerConfig(IPAddress.Any))
                    .AddTransientWithInterfaces<DashboardServer>()
                    .AddTransientWithInterfaces<WebSocketTelemetryNode>()
                ).Build();

            Console.WriteLine("Starting pipeline");

            var nodes = host.Services.GetServices<INode>();
            PipelineBuilder.LinkNodes(nodes);

            host.WaitForShutdown();
        }
    }
}
