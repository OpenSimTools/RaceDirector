using IGameTelemetry = RaceDirector.Pipeline.Telemetry.V0.IGameTelemetry;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using RaceDirector.Pipeline;
using RaceDirector.Plugin.HUD.Server;
using System.Collections.Generic;
using System;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Exposes live telemetry as a web socket server.
    /// </summary>
    public class WebSocketTelemetryNode : WebSocketNodeBase<IRunningGame, IGameTelemetry>, INode
    {
        public IObserver<IRunningGame> RunningGameTarget
        {
            get { return TriggerTarget; }
        }

        public IObserver<IGameTelemetry> GameTelemetryTarget
        {
            get { return DataTarget; }
        }

        public WebSocketTelemetryNode(IEnumerable<IWsServer<IGameTelemetry>> servers) : base(servers) { }

        override protected bool ServerShouldRun(IRunningGame runningGame) {
            return runningGame.IsRunning();
        }
    }
}
