using RaceDirector.Pipeline.Telemetry.V0;

namespace RaceDirector.PitCrew.Protocol;

public interface IPitStrategyRequest
{
    double? FuelToAddL { get; }
    uint? TireSet { get; }
    IPitStrategyTires? FrontTires { get; }
    IPitStrategyTires? RearTires { get; }
}

public interface IPitStrategyTires
{
    TireCompound Compound { get; }
    double? LeftPressureKpa { get; }
    double? RightPressureKpa { get; }
}
