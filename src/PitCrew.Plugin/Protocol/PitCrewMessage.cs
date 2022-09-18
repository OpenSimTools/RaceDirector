namespace RaceDirector.PitCrew.Protocol;

public record PitCrewMessage
(
    Telemetry? Telemetry,
    PitMenu? PitStrategyRequest
);

/// <summary>
/// Telemetry information
/// </summary>
/// <param name="Fuel">Fuel information</param>
public record Telemetry
(
    Fuel Fuel,
    PitMenu PitMenu
);

/// <summary>
/// Fuel information
/// </summary>
/// <param name="Left">Liters of fuel left in the tank</param>
public record Fuel
(
    double Left
);

/// <summary>
/// Pit menu
/// </summary>
/// <param name="FuelToAddL">Liters of fuel to add in the pit stop</param>
/// <param name="TireSet">Tire set for next pit stop, null for default</param>
/// <param name="TirePressuresKpa">Tire pressures in kpa (four-wheeled vehicles only), null for no tire change</param>
public record PitMenu
(
    double? FuelToAddL,
    int? TireSet,
    TireValues<double>? TirePressuresKpa
) : IPitStrategyRequest
{
    ITireValues<double>? IPitStrategyRequest.TirePressuresKpa => TirePressuresKpa;
};

public record TireValues<T>(T FL, T FR, T RL, T RR) : ITireValues<T>;