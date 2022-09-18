namespace RaceDirector.PitCrew.Protocol;

public record PitCrewMessage
(
    Telemetry? Telemetry,
    PitMenu? PitStrategyRequest
);

public record Telemetry
(
    double FuelLeftL,
    PitMenu PitMenu
);

public record PitMenu
(
    double? FuelToAddL,
    int? TireSet,
    PitMenuTires? FrontTires,
    PitMenuTires? RearTires
) : IPitStrategyRequest
{
    IPitMenuTires? IPitStrategyRequest.FrontTires => FrontTires;
    IPitMenuTires? IPitStrategyRequest.RearTires => RearTires;
};

public record PitMenuTires(
    // TODO Compound (some games and series allow different compounds per axle)
    double? LeftPressureKpa,
    double? RightPressureKpa
) : IPitMenuTires;