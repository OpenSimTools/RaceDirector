using IGameTelemetry = RaceDirector.Pipeline.Telemetry.V0.IGameTelemetry;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using RaceDirector.Pipeline;
using RaceDirector.HUD.Server;
using System.Collections.Generic;
using System;

namespace RaceDirector.HUD.Pipeline;

/// <summary>
/// Exposes live telemetry as a web socket server.
/// </summary>
public class WebSocketTelemetryNode : WebSocketNodeBase<IRunningGame, IGameTelemetry>, INode
{
    public IObserver<IRunningGame> RunningGameObserver => TriggerObserver;

    public IObserver<IGameTelemetry> GameTelemetryObserver => DataObserver;

    public WebSocketTelemetryNode(IEnumerable<IWsServer<IGameTelemetry>> servers) : base(servers) { }

    protected override bool ServerShouldRun(IRunningGame runningGame) {
        return runningGame.IsRunning();
    }
}