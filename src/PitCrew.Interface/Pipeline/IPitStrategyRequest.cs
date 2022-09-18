namespace RaceDirector.PitCrew.Protocol;

public interface IPitStrategyRequest
{
    double? FuelToAddL { get; }
    int? TireSet { get; }
    ITireValues<double>? TirePressuresKpa { get; }
}

public interface ITireValues<out T> {
    T FL { get; }
    T FR{ get; }
    T RL{ get; }
    T RR{ get; }
};