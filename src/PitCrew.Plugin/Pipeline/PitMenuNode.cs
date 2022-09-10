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
        var gameTelemetrySubject = new Subject<IGameTelemetry>();
        PitStrategyObserver = pitStrategySubject;
        GameTelemetryObserver = gameTelemetrySubject;
        GameActionObservable = pitStrategySubject
            .SelectMany(_ => todoSelectBasedOnGame.SetStrategy(_, gameTelemetrySubject));
    }
}