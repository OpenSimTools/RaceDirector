using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using System;

// TODO this is in the interface project to make testing easier, but it might be abused
namespace RaceDirector.Pipeline.Telemetry
{

    public record GameTelemetry
    (
        GameState GameState,
        Boolean UsingVR,
        IEvent? Event,
        ISession? Session,
        IVehicle[] Vehicles,
        IVehicle? CurrentVehicle,
        IPlayer? Player
    ) : IGameTelemetry;

    public record Event
    (
        ITrackLayout Track,
        Double FuelRate
    ) : IEvent;

    public record TrackLayout
    (
        IFraction<IDistance>[] SectorsEnd
    ) : ITrackLayout;

    public record Session
    (
        SessionType Type,
        SessionPhase Phase,
        ISessionDuration? Length,
        ISessionRequirements Requirements,
        ISpeed PitSpeedLimit,
        TimeSpan ElapsedTime,
        IStartLights? StartLights,
        ILapTime? BestLap,
        ISectors? BestSectors
    ) : ISession;

    public record SessionRequirements
    (
        UInt32 MandatoryPitStops,
        Interval<ISessionDuration>? PitWindow
    ) : ISessionRequirements;

    public record Vehicle
    (
        UInt32 Id,
        String DriverName,
        Int32 ClassPerformanceIndex,
        EngineType EngineType,
        ControlType ControlType,
        UInt32 PositionClass,
        TimeSpan GapAhead,
        TimeSpan GapBehind,
        ISectors? BestSectors,
        UInt32 CompletedLaps,
        Boolean CurrentLapValid,
        ILapTime? CurrentLapTime,
        ILapTime? PreviousLapTime,
        ILapTime? PersonalBestLapTime,
        IFraction<IDistance> CurrentLapDistance,
        Vector3<IDistance> Location,
        ISpeed Speed,
        IDriver CurrentDriver,
        ICounter MandatoryPitStops,
        IVehiclePit Pit
    ) : IVehicle;

    public record Player
    (
        IRawInputs RawInputs,
        IDrivingAids DrivingAids,
        IVehicleSettings VehicleSettings,
        IVehicleDamage VehicleDamage,
        ITyre[][] Tyres,
        IFuel Fuel,
        IEngine Engine,
        Vector3<IDistance> CgLocation,
        Orientation Orientation,
        Vector3<IAcceleration> Acceleration,
        ILapTime? ClassBestLap,
        ISectors? ClassBestSectors,
        ISectors? PersonalBestSectors,
        TimeSpan? PersonalBestDelta,
        IActivationToggled? Drs,
        IWaitTimeToggled? PushToPass,
        IPlayerPit? Pit,
        Flags GameFlags,
        Penalties Penalties
    ) : IPlayer;

    public record RawInputs
    (
        Double Steering,
        Double Throttle,
        Double Brake,
        Double Clutch,
        IAngle SteerWheelRange
    ) : IRawInputs;

    public record DrivingAids
    (
        UInt32? AbsLevelRaw,
        Double? AbsLevelPercent,
        Boolean AbsActive,
        UInt32? TcLevelRaw,
        Double? TcLevelPercent,
        Boolean TcActive,
        UInt32? EspLevelRaw,
        Double? EspLevelPercent,
        Boolean EspActive,
        Boolean? CountersteerEnabled,
        Boolean CountersteerActive,
        Boolean? CorneringEnabled,
        Boolean CorneringActive
    ) : IDrivingAids;

    public record VehicleSettings
    (
        UInt32? EngineMapRaw,
        Double? EngineMapPercent,
        UInt32? EngineBrakeRaw,
        Double? EngineBrakePercent
    ) : IVehicleSettings;

    public record VehicleDamage
    (
        Double AerodynamicsPercent,
        Double EnginePercent,
        Double SuspensionPercent,
        Double TransmissionPercent
    ) : IVehicleDamage;

    public record Tyre
    (
        Double Dirt,
        Double Grip,
        Double Wear,
        ITemperaturesMatrix Temperatures,
        ITemperaturesSingle BrakeTemperatures
    ) : ITyre;

    public record TemperaturesSingle
    (
        ITemperature CurrentTemperature,
        ITemperature OptimalTemperature,
        ITemperature ColdTemperature,
        ITemperature HotTemperature
    ) : ITemperaturesSingle;

    public record TemperaturesMatrix
    (
        ITemperature[][] CurrentTemperatures,
        ITemperature OptimalTemperature,
        ITemperature ColdTemperature,
        ITemperature HotTemperature
    ) : ITemperaturesMatrix;

    public record Fuel
    (
        Double Max,
        Double Left,
        Double? PerLap
    ) : IFuel;

    public record Engine
    (
        IAngularSpeed Speed,
        IAngularSpeed UpshiftSpeed,
        IAngularSpeed MaxSpeed
    ) : IEngine;

    public record PlayerPit
    (
        PitState State,
        PitAction Action,
        TimeSpan DurationTotal,
        TimeSpan DurationLeft
    ) : IPlayerPit;
}
