namespace RaceDirector.PitCrew.Protocol;

/// <summary>
/// Telemetry information
/// </summary>
/// <param name="Fuel">Fuel information</param>
public record Telemetry(Fuel Fuel);

/// <summary>
/// Fuel information
/// </summary>
/// <param name="Left">Liters of fuel left in the tank</param>
public record Fuel(double? Left);