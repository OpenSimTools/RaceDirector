using IGameTelemetry = RaceDirector.Pipeline.Telemetry.V0.IGameTelemetry;
using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;
using RaceDirector.Pipeline;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Server;

namespace RaceDirector.Remote.Pipeline;

/// <summary>
/// Exposes live telemetry as a web socket server.
/// </summary>
public class WebSocketTelemetryNode : WebSocketNodeBase<IRunningGame, IGameTelemetry>, INode
{
    public WebSocketTelemetryNode(IEnumerable<IWsServer<IGameTelemetry>> servers,
            IEnumerable<IWsClient<IGameTelemetry>> clients) :
        base(servers.Select(s => s.ToRemotePublisher())
            .Concat(clients.Select(c => c.ToRemotePublisher())))
    {
    }

    public IObserver<IRunningGame> RunningGameObserver => TriggerObserver;

    public IObserver<IGameTelemetry> GameTelemetryObserver => DataObserver;

    protected override bool PublisherShouldStart(IRunningGame runningGame) {
        return runningGame.IsRunning();
    }
}