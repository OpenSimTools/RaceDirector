using System.Reactive.Linq;
using System.Reactive.Subjects;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline;

public class PitMenuNode : INode
{
    public IObserver<IPitStrategyRequest> PitStrategyObserver { get; }

    public IObserver<IGameTelemetry> GameTelemetryObserver { get; }

    public IObservable<GameAction> GameActionObservable { get; }

    public PitMenuNode(IGamePitMenuNavigator todoSelectBasedOnGame)
    {
        var pitStrategySubject = new Subject<IPitStrategyRequest>();
        // Cache last value and publish on subscription
        var gameTelemetrySubject = new ReplaySubject<IGameTelemetry>(1);
        PitStrategyObserver = pitStrategySubject;
        GameTelemetryObserver = gameTelemetrySubject;
        GameActionObservable = pitStrategySubject
            .SelectMany(_ => todoSelectBasedOnGame.SetStrategy(_, gameTelemetrySubject));
    }
}