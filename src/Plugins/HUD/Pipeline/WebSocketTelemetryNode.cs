using IGameTelemetry = RaceDirector.Pipeline.Telemetry.V0.IGameTelemetry;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using System.Threading.Tasks.Dataflow;
using RaceDirector.Pipeline;
using System.Collections.Generic;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Exposes live telemetry as a web socket server.
    /// </summary>
    public class WebSocketTelemetryNode : WebSocketNodeBase<IRunningGame, IGameTelemetry>, INode
    {
        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get { return TriggerTarget; }
        }

        public ITargetBlock<IGameTelemetry> GameTelemetryTarget
        {
            get { return DataTarget; }
        }

        public WebSocketTelemetryNode(IEnumerable<IWsServer<IGameTelemetry>> servers) : base(servers) { }

        override protected bool ServerShouldRun(IRunningGame runningGame) {
            return runningGame.IsRunning();
        }
    }
}
