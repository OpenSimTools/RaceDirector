using System.Reactive.Linq;
using RaceDirector.Pipeline;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline;

public class PitCrewNode : INode
{
    public IObservable<IPitStrategyRequest> PitStrategyObservable { get; }

    public PitCrewNode(PitCrewClient client)
    {
        PitStrategyObservable = client.In.SelectMany(psr =>
            psr is null ?
                Observable.Empty<IPitStrategyRequest>() :
                Observable.Return(psr)
        );
    }
}