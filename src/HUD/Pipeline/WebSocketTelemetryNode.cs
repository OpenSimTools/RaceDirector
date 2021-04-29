using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    using ILiveTelemetry = RaceDirector.Pipeline.Telemetry.V0.ILiveTelemetry;
    using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;

    public class WebSocketTelemetryNode
    {
        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get;
        }

        public ITargetBlock<ILiveTelemetry> LiveTelemetryTarget
        {
            get;
        }
    }
}
