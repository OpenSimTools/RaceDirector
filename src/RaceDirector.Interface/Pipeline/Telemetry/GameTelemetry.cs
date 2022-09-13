using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using System;
using System.Linq;
using static System.Array;
using static RaceDirector.Pipeline.Telemetry.V0.IVehicleFlags;

// TODO this is in the interface project to make testing easier, but it might be abused
namespace RaceDirector.Pipeline.Telemetry;

public record GameTelemetry
(
    GameState GameState,
    bool? UsingVR,
    Event? Event,
    Session? Session,
    Vehicle[] Vehicles,
    Vehicle? FocusedVehicle,
    Player? Player
) : IGameTelemetry
{
    IEvent? IGameTelemetry.Event => Event;
    ISession? IGameTelemetry.Session => Session;
    IVehicle[] IGameTelemetry.Vehicles => Vehicles;
    IFocusedVehicle? IGameTelemetry.FocusedVehicle => FocusedVehicle;
    IPlayer? IGameTelemetry.Player => Player;

    public static GameTelemetry Empty = new(
        GameState.Unknown,
        UsingVR: null,
        Event: null,
        Session: null,
        Vehicles: Empty<Vehicle>(),
        FocusedVehicle: null,
        Player: null
    );
}

public record Event
(
    TrackLayout TrackLayout,
    // ISessionDuration[] SessionsLength
    double FuelRate
) : IEvent
{
    ITrackLayout IEvent.TrackLayout => TrackLayout;
}

public record TrackLayout
(
    IFraction<IDistance>[] SectorsEnd
) : ITrackLayout
{
    IFraction<IDistance>[] ITrackLayout.SectorsEnd => SectorsEnd;
}

public record Session
(
    SessionType Type,
    SessionPhase Phase,
    ISessionDuration? Length,
    SessionRequirements Requirements,
    ISpeed PitSpeedLimit,
    bool PitLaneOpen,
    TimeSpan? ElapsedTime,
    TimeSpan? TimeRemaining,
    TimeSpan? WaitTime,
    StartLights? StartLights,
    LapTime? BestLap,
    Sectors? BestSectors,
    SessionFlags Flags
) : ISession
{
    ISessionRequirements ISession.Requirements => Requirements;
    IStartLights? ISession.StartLights => StartLights;
    ILapTime? ISession.BestLap => BestLap;
    ISectors? ISession.BestSectors => BestSectors;
    ISessionFlags ISession.Flags => Flags;
}

public record SessionRequirements
(
    uint MandatoryPitStops,
    MandatoryPitRequirements MandatoryPitRequirements,
    Interval<IPitWindowBoundary>? PitWindow
) : ISessionRequirements;

public record StartLights(
    LightColor Color,
    BoundedValue<uint> Lit
) : IStartLights
{
    IBoundedValue<uint> IStartLights.Lit => Lit;
}

public record SessionFlags(
    TrackFlags Track,
    SectorFlags[] Sectors,
    LeaderFlags Leader
) : ISessionFlags;

public record Vehicle
(
    uint Id,
    int ClassPerformanceIndex,
    IRacingStatus RacingStatus,
    EngineType EngineType,
    ControlType ControlType,
    uint Position,
    uint PositionClass,
    TimeSpan? GapAhead,
    TimeSpan? GapBehind,
    uint CompletedLaps,
    LapValidState LapValid,
    LapTime? CurrentLapTime,
    LapTime? PreviousLapTime,
    LapTime? BestLapTime,
    Sectors? BestSectors,
    IFraction<IDistance> CurrentLapDistance,
    Vector3<IDistance> Location,
    Orientation? Orientation,
    ISpeed Speed,
    Driver CurrentDriver,
    VehiclePit Pit,
    Penalty[] Penalties,
    VehicleFlags Flags,
    Inputs? Inputs
) : IFocusedVehicle
{
    ILapTime? IVehicle.CurrentLapTime => CurrentLapTime;
    ILapTime? IVehicle.PreviousLapTime => PreviousLapTime;
    ILapTime? IVehicle.BestLapTime => BestLapTime;
    ISectors? IVehicle.BestSectors => BestSectors;
    IDriver IVehicle.CurrentDriver => CurrentDriver;
    IVehiclePit IVehicle.Pit => Pit;
    IVehicleFlags IVehicle.Flags => Flags;
    IPenalty[] IVehicle.Penalties => Penalties;
    IInputs? IFocusedVehicle.Inputs => Inputs;
}

public record Driver
(
    String Name
) : IDriver;

public record VehiclePit
(
    uint StopsDone,
    uint MandatoryStopsDone,
    PitLanePhase? PitLanePhase,
    TimeSpan? PitLaneTime,
    TimeSpan? PitStallTime
) : IVehiclePit;

public record Penalty
(
    PenaltyType Type,
    PenaltyReason Reason
) : IPenalty;

public record Inputs
(
    double Throttle,
    double Brake,
    double Clutch
) : IInputs;

public record VehicleFlags
(
    GreenFlag? Green,
    BlueFlag? Blue,
    YellowFlag? Yellow,
    WhiteFlag? White,
    Flag? Checkered,
    Flag? Black,
    BlackWhiteFlag? BlackWhite
) : IVehicleFlags
{
    IGreen? IVehicleFlags.Green => Green;
    IBlue? IVehicleFlags.Blue => Blue;
    IYellow? IVehicleFlags.Yellow => Yellow;
    IWhite? IVehicleFlags.White => White;
    IFlag? IVehicleFlags.Checkered => Checkered;
    IFlag? IVehicleFlags.Black => Black;
    IBlackWhite? IVehicleFlags.BlackWhite => BlackWhite;
}

public record GreenFlag(GreenReason Reason) : IGreen;
public record BlueFlag(BlueReason Reason) : IBlue;
public record YellowFlag(YellowReason Reason) : IYellow;
public record WhiteFlag(WhiteReason Reason) : IWhite;
public record BlackWhiteFlag(BlackWhiteReason Reason) : IBlackWhite;
public record Flag() : IFlag;

public record Player
(
    RawInputs RawInputs,
    DrivingAids DrivingAids,
    VehicleSettings VehicleSettings,
    VehicleDamage VehicleDamage,
    Tire[][] Tires,
    Fuel Fuel,
    Engine Engine,
    Vector3<IDistance> CgLocation,
    Orientation Orientation,
    Vector3<IAcceleration> LocalAcceleration,
    LapTime? ClassBestLap,
    Sectors? ClassBestSectors,
    Sectors? PersonalBestSectors,
    TimeSpan? PersonalBestDelta,
    ActivationToggled? Drs,
    WaitTimeToggled? PushToPass,
    PlayerPitStopStatus PitStopStatus,
    PlayerWarnings Warnings,
    bool? OvertakeAllowed,
    PitMenu PitMenu
) : IPlayer
{
    IRawInputs IPlayer.RawInputs => RawInputs;
    IDrivingAids IPlayer.DrivingAids => DrivingAids;
    IVehicleSettings IPlayer.VehicleSettings => VehicleSettings;
    IVehicleDamage IPlayer.VehicleDamage => VehicleDamage;
    ITire[][] IPlayer.Tires => Tires;
    IFuel IPlayer.Fuel => Fuel;
    IEngine IPlayer.Engine => Engine;
    ILapTime? IPlayer.ClassBestLap => ClassBestLap;
    ISectors? IPlayer.ClassBestSectors => ClassBestSectors;
    ISectors? IPlayer.PersonalBestSectors => PersonalBestSectors;
    IActivationToggled? IPlayer.Drs => Drs;
    IWaitTimeToggled? IPlayer.PushToPass => PushToPass;
    PlayerPitStopStatus IPlayer.PitStopStatus => PitStopStatus;
    IPlayerWarnings IPlayer.Warnings => Warnings;
    IPitMenu IPlayer.PitMenu => PitMenu;
}

public record RawInputs
(
    double Throttle,
    double Brake,
    double Clutch,
    double Steering,
    IAngle SteerWheelRange
) : IRawInputs;

public record DrivingAids(
    Aid? Abs,
    TractionControl? Tc,
    Aid? Esp,
    Aid? Countersteer,
    Aid? Cornering
) : IDrivingAids
{
    IAid? IDrivingAids.Abs => Abs;
    ITractionControl? IDrivingAids.Tc => Tc;
    IAid? IDrivingAids.Esp => Esp;
    IAid? IDrivingAids.Countersteer => Countersteer;
    IAid? IDrivingAids.Cornering => Cornering;
}

public record TractionControl
(
    uint Level,
    bool Active,
    uint? Cut
) : Aid(Level, Active), ITractionControl;

public record Aid
(
    uint Level,
    bool Active
) : IAid;

public record VehicleSettings
(
    uint? EngineMap,
    uint? EngineBrakeReduction
) : IVehicleSettings;

public record VehicleDamage
(
    double AerodynamicsPercent,
    double EnginePercent,
    double SuspensionPercent,
    double TransmissionPercent
) : IVehicleDamage;

public record Tire
(
    double Dirt,
    double Grip,
    double Wear,
    TemperaturesMatrix Temperatures,
    TemperaturesSingle BrakeTemperatures
) : ITire
{
    ITemperaturesMatrix ITire.Temperatures => Temperatures;
    ITemperaturesSingle ITire.BrakeTemperatures => BrakeTemperatures;
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
    ICapacity Max,
    ICapacity Left,
    ICapacity? PerLap
) : IFuel;

public record Engine
(
    IAngularSpeed Speed,
    IAngularSpeed UpshiftSpeed,
    IAngularSpeed MaxSpeed
) : IEngine;

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
    bool Available,
    bool Engaged,
    IBoundedValue<uint>? ActivationsLeft // R3E Drs.NumActivationsLeft + DrsNumActivationsTotal
) : IActivationToggled;

public record WaitTimeToggled
(
    bool Available,
    bool Engaged,
    IBoundedValue<uint>? ActivationsLeft, // R3E PushToPass.AmountLeft + PtpNumActivationsTotal
    TimeSpan EngagedTimeLeft,
    TimeSpan WaitTimeLeft
) : IWaitTimeToggled;

public record PlayerWarnings
(
    IBoundedValue<uint>? IncidentPoints,
    IBoundedValue<uint>? BlueFlagWarnings,
    uint GiveBackPositions
) : IPlayerWarnings;

public record PitMenu
(
    PitMenuFocusedItem FocusedItem,
    PitMenuSelectedItems SelectedItems,
    ICapacity? FuelToAdd
) : IPitMenu;

public static class DistanceFraction
{
    public static IFraction<IDistance> Of(IDistance value, double fraction) =>
        new OfFraction(value, fraction);

    public static IFraction<IDistance> FromTotal(IDistance total, IDistance value) =>
        new OfTotalValue(total, value);

    public static IFraction<IDistance> FromTotal(IDistance total, double fraction) =>
        new OfTotalFraction(total, fraction);

    public static IFraction<IDistance>[] FromTotal(IDistance total, params double[] fractions) =>
        fractions.Select(f => new OfTotalFraction(total, f)).ToArray();

    private record OfFraction(IDistance Value, double Fraction) : IFraction<IDistance>
    {
        private Lazy<IDistance> _LazyTotal = new Lazy<IDistance>(() => Value / Fraction);

        public IDistance Total => _LazyTotal.Value;
    }

    private record OfTotalFraction(IDistance Total, double Fraction) : IFraction<IDistance>
    {
        private Lazy<IDistance> _LazyValue = new Lazy<IDistance>(() => Total * Fraction);

        public IDistance Value => _LazyValue.Value;
    }

    private record OfTotalValue(IDistance Total, IDistance Value) : IFraction<IDistance>
    {
        private Lazy<double> _LazyValue = new Lazy<double>(() => Value / Total);

        public double Fraction => _LazyValue.Value;
    }
}

public record BoundedValue<T>(T Value, T Total) : IBoundedValue<T>;

public record RaceInstant(TimeSpan? Time, uint Laps) : IRaceInstant
{
    public bool IsWithin<T>(Interval<T> boundary) where T : IComparable<IRaceInstant> =>
        boundary.Start.CompareTo(this) <= 0 && boundary.Finish.CompareTo(this) > 0;
}