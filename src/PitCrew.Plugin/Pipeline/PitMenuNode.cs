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
        GameActionObservable = subject.Select(_ => GameAction.PitMenuOpen);
    }
}