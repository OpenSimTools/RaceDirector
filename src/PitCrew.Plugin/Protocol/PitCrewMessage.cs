using RaceDirector.Pipeline.Telemetry.V0;

namespace RaceDirector.PitCrew.Protocol;

public record PitCrewMessage
(
    Telemetry? Telemetry,
    PitMenu? PitStrategyRequest
);

public record Telemetry
(
    double FuelLeftL,
    uint? TireSet,
    TelemetryTireAxle? FrontTires,
    TelemetryTireAxle? RearTires,
    PitMenu PitMenu
);

public record TelemetryTireAxle
(
    TireCompound Compound, 
    TelemetryTire Left,
    TelemetryTire Right
);

public record TelemetryTire
(
    double? PressureKpa,
    double Wear
);

public record PitMenu(
    double? FuelToAddL,
    uint? TireSet,
    PitMenuTires? FrontTires,
    PitMenuTires? RearTires
) : IPitStrategyRequest
{
    IPitStrategyTires? IPitStrategyRequest.FrontTires => FrontTires;
    IPitStrategyTires? IPitStrategyRequest.RearTires => RearTires;
}

public record PitMenuTires(
    TireCompound Compound,
    double? LeftPressureKpa,
    double? RightPressureKpa
) : IPitStrategyTires;