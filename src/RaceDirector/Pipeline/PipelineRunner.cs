using RaceDirector.Pipeline.GameMonitor;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using RaceDirector.Pipeline.Telemetry;
using ILiveTelemetry = RaceDirector.Pipeline.Telemetry.V0.ILiveTelemetry;
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

            var telemetryLogger = new ActionBlock<ILiveTelemetry>(liveTelemetry =>
                Console.WriteLine("> " + liveTelemetry.SimulationTime.TotalSeconds)
            );

            var dashboardServer = new DashboardServer(new DashboardServer.Config(IPAddress.Any));
            var telemetryServer = new WebSocketTelemetryNode(dashboardServer);

            var runningGameBroadcast = new BroadcastBlock<IRunningGame>(null);
            var telemetryBroadcast = new BroadcastBlock<ILiveTelemetry>(null);

            // Linking

            processMonitorNode.RunningGameSource.LinkTo(runningGameBroadcast);

            runningGameBroadcast.LinkTo(telemetryReaderNode.RunningGameTarget);
            runningGameBroadcast.LinkTo(telemetryServer.RunningGameTarget);

            telemetryReaderNode.LiveTelemetrySource.LinkTo(telemetryBroadcast);

            telemetryBroadcast.LinkTo(telemetryServer.LiveTelemetryTarget);
            telemetryBroadcast.LinkTo(telemetryLogger);

            return processMonitorNode.RunningGameSource.Completion;
        }
    }
}
