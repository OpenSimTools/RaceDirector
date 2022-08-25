using RaceDirector.Pipeline;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline;

public class PitCrewNode : INode
{
    public IObservable<IRequestPitStrategy> PitStrategyObservable { get; }

    public PitCrewNode(PitCrewClient client)
    {
        PitStrategyObservable = client.In;
    }
}