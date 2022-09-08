using System.Reactive.Linq;
using System.Reactive.Subjects;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline;

public class PitMenuNode : INode
{
    public IObserver<IPitStrategyRequest> PitStrategyObserver { get; }
    public IObservable<GameAction> GameActionObservable { get; }

    public PitMenuNode()
    {
        var subject = new Subject<IPitStrategyRequest>();
        PitStrategyObserver = subject;
        GameActionObservable = subject.SelectMany(psr =>
            // TODO This is for ACC. We should have a generic interface
            new[] { GameAction.PitMenuOpen, GameAction.PitMenuDown, GameAction.PitMenuDown }
                .ToObservable()
                .Concat(Observable.Repeat(GameAction.PitMenuRight, psr.FuelToAdd))
        );
    }
}