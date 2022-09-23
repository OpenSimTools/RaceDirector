namespace RaceDirector.PitCrew.Protocol;

public interface IPitStrategyRequest
{
    double? FuelToAddL { get; }
    uint? TireSet { get; }
    IPitMenuTires? FrontTires { get; }
    IPitMenuTires? RearTires { get; }
}

public interface IPitMenuTires {
    double? LeftPressureKpa { get; }
    double? RightPressureKpa { get; }
}