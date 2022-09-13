namespace RaceDirector.PitCrew.Protocol;

public record PitCrewMessage
(
    Telemetry? Telemetry,
    PitStrategyRequest? PitStrategyRequest
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
/// <param name="FuelToAdd">Liters of fuel to add in the pit stop</param>
public record PitMenu
(
    double? FuelToAdd
);

public record PitStrategyRequest(double FuelToAdd) : IPitStrategyRequest;