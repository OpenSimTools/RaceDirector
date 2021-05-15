using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Plugin.HUD.Pipeline;
using System;
using System.Net;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline
{
    [SupportedOSPlatform("windows")]
    public class PipelineRunner
    {
        /// <summary>
        /// Constructs and runs the whole pipeline.
        /// </summary>
        /// <remarks>
        /// Not currently disposing anything because it's just
        /// a test and it will be completely replaced.
        /// </remarks>
        public Task Run()
        {
            var games = new[] {
                new Games.R3E.Game(new Games.R3E.Game.Config(TimeSpan.FromMilliseconds(500)))
            };

            var processMonitorNode = new ProcessMonitorNode(
                new ProcessMonitorNode.Config(TimeSpan.FromSeconds(5)),
                games
            );

            var telemetryReaderNode = new TelemetryReaderNode(games);

            var telemetryLoggerNode = new TelemetryLoggerNode();

            var dashboardServer = new DashboardServer(new DashboardServer.Config(IPAddress.Any));
            var telemetryServerNode = new WebSocketTelemetryNode(dashboardServer);

            PipelineBuilder.LinkNodes(processMonitorNode, telemetryReaderNode, telemetryLoggerNode, telemetryServerNode);

            return processMonitorNode.RunningGameSource.Completion;
        }
    }

    public class TelemetryLoggerNode : INode, IDisposable
    {
        public ITargetBlock<Telemetry.V0.ILiveTelemetry> LiveTelemetryTarget =>
            new ActionBlock<Telemetry.V0.ILiveTelemetry>(liveTelemetry =>
                Console.WriteLine("> " + liveTelemetry.SimulationTime.TotalSeconds)
            );

        public void Dispose()
        {
            LiveTelemetryTarget.Complete();
        }
    }
}
