using System.Reactive;
using System.Reactive.Subjects;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline;

public class PitStrategyNode : INode
{
    public IObserver<IPitStrategyRequest> PitStrategyIn { get; }
    
    public IObservable<GameAction> GameActionOut { get; }

    public PitStrategyNode()
    {
        var gameActionSubject = new Subject<GameAction>();
        PitStrategyIn = Observer.Create<IPitStrategyRequest>(_ => gameActionSubject.OnNext(GameAction.PitMenuUp));
        GameActionOut = gameActionSubject;
    }
}