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
        Event? Event,
        Session? Session,
        Vehicle[] Vehicles,
        Vehicle? CurrentVehicle,
        Player? Player
    ) : IGameTelemetry
    {
        IEvent? IGameTelemetry.Event => Event;
        ISession? IGameTelemetry.Session => Session;
        IVehicle[] IGameTelemetry.Vehicles => Vehicles;
        IVehicle? IGameTelemetry.CurrentVehicle => CurrentVehicle;
        IPlayer? IGameTelemetry.Player => Player;
    }

    public record Event
    (
        TrackLayout Track,
        Double FuelRate
    ) : IEvent
    {
        ITrackLayout IEvent.Track => Track;
    }

    public record TrackLayout
    (
        IFraction<IDistance>[] SectorsEnd
    ) : ITrackLayout;

    public record Session
    (
        SessionType Type,
        SessionPhase Phase,
        ISessionDuration? Length,
        SessionRequirements Requirements,
        ISpeed PitSpeedLimit,
        TimeSpan ElapsedTime,
        StartLights? StartLights,
        LapTime? BestLap,
        Sectors? BestSectors
    ) : ISession
    {
        ISessionRequirements ISession.Requirements => Requirements;
        IStartLights? ISession.StartLights => StartLights;
        ILapTime? ISession.BestLap => BestLap;
        ISectors? ISession.BestSectors => BestSectors;
    }

    public record SessionRequirements
    (
        UInt32 MandatoryPitStops,
        Interval<ISessionDuration>? PitWindow
    ) : ISessionRequirements;

    public record StartLights(
        LightColour Colour,
        BoundedValue<UInt32> Lit
    ) : IStartLights
    {
        IBoundedValue<uint> IStartLights.Lit => Lit;
    }

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
        Sectors? BestSectors,
        UInt32 CompletedLaps,
        Boolean CurrentLapValid,
        LapTime? CurrentLapTime,
        LapTime? PreviousLapTime,
        LapTime? PersonalBestLapTime,
        IFraction<IDistance> CurrentLapDistance,
        Vector3<IDistance> Location,
        ISpeed Speed,
        Driver CurrentDriver,
        Counter MandatoryPitStops,
        VehiclePit Pit
    ) : IVehicle
    {
        ILapTime? IVehicle.CurrentLapTime => CurrentLapTime;
        ILapTime? IVehicle.PreviousLapTime => PreviousLapTime;
        ILapTime? IVehicle.PersonalBestLapTime => PersonalBestLapTime;
        ISectors? IVehicle.BestSectors => BestSectors;
        IDriver IVehicle.CurrentDriver => CurrentDriver;
        ICounter IVehicle.MandatoryPitStops => MandatoryPitStops;
        IVehiclePit IVehicle.Pit => Pit;
    }

    public record Driver(
        String Name
    ) : IDriver;

    public record Counter(
        UInt32 Total,
        UInt32 Left,
        UInt32 Done
    ) : ICounter;

    public record VehiclePit(
        Boolean InPitLane,
        TimeSpan? PitLaneTime,
        Boolean? InPitStall,
        TimeSpan? PitStallTime
    ) : IVehiclePit;

    public record Player
    (
        RawInputs RawInputs,
        DrivingAids DrivingAids,
        VehicleSettings VehicleSettings,
        VehicleDamage VehicleDamage,
        Tyre[][] Tyres,
        Fuel Fuel,
        Engine Engine,
        Vector3<IDistance> CgLocation,
        Orientation Orientation,
        Vector3<IAcceleration> Acceleration,
        LapTime? ClassBestLap,
        Sectors? ClassBestSectors,
        Sectors? PersonalBestSectors,
        TimeSpan? PersonalBestDelta,
        ActivationToggled? Drs,
        WaitTimeToggled? PushToPass,
        PlayerPit? Pit,
        Flags GameFlags,
        Penalties Penalties
    ) : IPlayer
    {
        IRawInputs IPlayer.RawInputs => RawInputs;
        IDrivingAids IPlayer.DrivingAids => DrivingAids;
        IVehicleSettings IPlayer.VehicleSettings => VehicleSettings;
        IVehicleDamage IPlayer.VehicleDamage => VehicleDamage;
        ITyre[][] IPlayer.Tyres => Tyres;
        IFuel IPlayer.Fuel => Fuel;
        IEngine IPlayer.Engine => Engine;
        ILapTime? IPlayer.ClassBestLap => ClassBestLap;
        ISectors? IPlayer.ClassBestSectors => ClassBestSectors;
        ISectors? IPlayer.PersonalBestSectors => PersonalBestSectors;
        IActivationToggled? IPlayer.Drs => Drs;
        IWaitTimeToggled? IPlayer.PushToPass => PushToPass;
        IPlayerPit? IPlayer.Pit => Pit;
    }

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
        TemperaturesMatrix Temperatures,
        TemperaturesSingle BrakeTemperatures
    ) : ITyre
    {
        ITemperaturesMatrix ITyre.Temperatures => Temperatures;
        ITemperaturesSingle ITyre.BrakeTemperatures => BrakeTemperatures;
    }

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

    public record LapTime(
        TimeSpan Overall,
        Sectors Sectors
    ) : ILapTime
    {
        ISectors ILapTime.Sectors => Sectors;
    }

    public record Sectors(
        TimeSpan[] Individual,
        TimeSpan[] Cumulative
    ) : ISectors;

    public record ActivationToggled
    (
        Boolean Available,
        Boolean Engaged,
        UInt32 ActivationsLeft
    ) : IActivationToggled;

    public record WaitTimeToggled
    (
        Boolean Available,
        Boolean Engaged,
        UInt32 ActivationsLeft,
        TimeSpan EngagedTimeLeft,
        TimeSpan WaitTimeLeft
    ) : IWaitTimeToggled;

    public record BoundedValue<T>(T Total, T Value) : IBoundedValue<T>;
}
