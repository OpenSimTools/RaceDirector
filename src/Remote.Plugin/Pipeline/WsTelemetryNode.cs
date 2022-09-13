using IGameTelemetry = RaceDirector.Pipeline.Telemetry.V0.IGameTelemetry;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using RaceDirector.Pipeline;
using RaceDirector.Remote.Networking;

namespace RaceDirector.Remote.Pipeline;

/// <summary>
/// Exposes live telemetry as a web socket server.
/// </summary>
public class WsTelemetryNode : WsNodeBase<IRunningGame, IGameTelemetry>, INode
{
    public WsTelemetryNode(IEnumerable<IStartableConsumer<IGameTelemetry>> startable, IEnumerable<IConnectableConsumer<IGameTelemetry>> connectable) :
        base(startable.Select(s => s.ToRemotePusher())
        .Concat(connectable.Select(c => c.ToRemotePusher())))
    {
    }

    public IObserver<IRunningGame> RunningGameObserver => TriggerObserver;

    public IObserver<IGameTelemetry> GameTelemetryObserver => DataObserver;

    protected override bool PusherShouldStart(IRunningGame runningGame) {
        return runningGame.IsRunning();
    }
}