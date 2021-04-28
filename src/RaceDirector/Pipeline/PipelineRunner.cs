using RaceDirector.Pipeline.GameMonitor;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline
{
    [SupportedOSPlatform("windows")]
    public class PipelineRunner
    {
        /// <summary>
        /// Constructs and run the whole pipeline.
        /// </summary>
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

            processMonitorNode.RunningGameSource.LinkTo(telemetryReaderNode.RunningGameTarget);
            telemetryReaderNode.LiveTelemetrySource.LinkTo(telemetryLogger);

            return processMonitorNode.RunningGameSource.Completion;
        }
    }
}
