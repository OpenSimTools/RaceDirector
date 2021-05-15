using ILiveTelemetry = RaceDirector.Pipeline.Telemetry.V0.ILiveTelemetry;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using System.Threading.Tasks.Dataflow;
using RaceDirector.Pipeline;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Exposes live telemetry as a web socket server.
    /// </summary>
    public class WebSocketTelemetryNode : WebSocketNodeBase<IRunningGame, ILiveTelemetry>, INode
    {
        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get { return TriggerTarget; }
        }

        public ITargetBlock<ILiveTelemetry> LiveTelemetryTarget
        {
            get { return DataTarget; }
        }

        public WebSocketTelemetryNode(params IWsServer<ILiveTelemetry>[] servers) : base(servers) { }

        override protected bool ServerShouldRun(IRunningGame runningGame) {
            return runningGame.IsRunning();
        }
    }
}
