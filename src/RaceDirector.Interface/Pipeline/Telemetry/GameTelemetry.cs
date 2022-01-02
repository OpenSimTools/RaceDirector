using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using System;
using System.Linq;
using static RaceDirector.Pipeline.Telemetry.V0.IVehicleFlags;

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
        Vehicle? FocusedVehicle,
        Player? Player
    ) : IGameTelemetry
    {
        IEvent? IGameTelemetry.Event => Event;
        ISession? IGameTelemetry.Session => Session;
        IVehicle[] IGameTelemetry.Vehicles => Vehicles;
        IFocusedVehicle? IGameTelemetry.FocusedVehicle => FocusedVehicle;
        IPlayer? IGameTelemetry.Player => Player;
    }

    public record Event
    (
        TrackLayout Track,
        // ISessionDuration[] SessionsLength
        Double FuelRate
    ) : IEvent
    {
        ITrackLayout IEvent.Track => Track;
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
        TimeSpan ElapsedTime,
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
        UInt32 MandatoryPitStops,
        MandatoryPitRequirements MandatoryPitRequirements,
        Interval<IPitWindowBoundary>? PitWindow
    ) : ISessionRequirements;

    public record StartLights(
        LightColour Colour,
        BoundedValue<UInt32> Lit
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
        UInt32 Id,
        Int32 ClassPerformanceIndex,
        IRacingStatus RacingStatus,
        EngineType EngineType,
        ControlType ControlType,
        UInt32 Position,
        UInt32 PositionClass,
        TimeSpan? GapAhead,
        TimeSpan? GapBehind,
        UInt32 CompletedLaps,
        Boolean CurrentLapValid,
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
        Inputs? Inputs,
        VehicleFlags Flags
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
        UInt32 StopsDone,
        UInt32 MandatoryStopsDone,
        PitLaneState? PitLaneState,
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
        Double Throttle,
        Double Brake,
        Double Clutch
    ) : IInputs;

    public record VehicleFlags
    (
        GreenFlag? Green,
        BlueFlag? Blue,
        YellowFlag? Yellow,
        WhiteFlag? White,
        Flag? Chequered,
        Flag? Black,
        BlackWhiteFlag? BlackWhite
    ) : IVehicleFlags
    {
        IGreen? IVehicleFlags.Green => Green;
        IBlue? IVehicleFlags.Blue => Blue;
        IYellow? IVehicleFlags.Yellow => Yellow;
        IWhite? IVehicleFlags.White => White;
        IFlag? IVehicleFlags.Chequered => Chequered;
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
        Tyre[][] Tyres,
        Fuel Fuel,
        Engine Engine,
        Vector3<IDistance> CgLocation,
        Orientation Orientation,
        Vector3<IAcceleration> LocalAcceleration,
        LapValidState LapValid,
        LapTime? ClassBestLap,
        Sectors? ClassBestSectors,
        Sectors? PersonalBestSectors,
        TimeSpan? PersonalBestDelta,
        ActivationToggled? Drs,
        WaitTimeToggled? PushToPass,
        PlayerPitStop PitStop,
        PlayerWarnings Warnings,
        Boolean? OvertakeAllowed
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
        PlayerPitStop IPlayer.PitStop => PitStop;
        IPlayerWarnings IPlayer.Warnings => Warnings;
    }

    public record RawInputs
    (
        Double Throttle,
        Double Brake,
        Double Clutch,
        Double Steering,
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
        UInt32 Level,
        Boolean Active,
        UInt32? Cut
    ) : Aid(Level, Active), ITractionControl;

    public record Aid
    (
        UInt32 Level,
        Boolean Active
    ) : IAid;

    public record VehicleSettings
    (
        UInt32? EngineMap,
        UInt32? EngineBrakeReduction
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
        IBoundedValue<UInt32>? ActivationsLeft // R3E Drs.NumActivationsLeft + DrsNumActivationsTotal
    ) : IActivationToggled;

    public record WaitTimeToggled
    (
        Boolean Available,
        Boolean Engaged,
        IBoundedValue<UInt32>? ActivationsLeft, // R3E PushToPass.AmountLeft + PtpNumActivationsTotal
        TimeSpan EngagedTimeLeft,
        TimeSpan WaitTimeLeft
    ) : IWaitTimeToggled;

    public record PlayerWarnings
    (
        IBoundedValue<UInt32>? IncidentPoints,
        IBoundedValue<UInt32>? BlueFlagWarnings,
        UInt32 GiveBackPositions
    ) : IPlayerWarnings;

public static class DistanceFraction
    {
        public static IFraction<IDistance> Of(IDistance value, Double fraction) =>
            new OfFraction(value, fraction);

        public static IFraction<IDistance> FromTotal(IDistance total, IDistance value) =>
            new OfTotalValue(total, value);

        public static IFraction<IDistance> FromTotal(IDistance total, Double fraction) =>
            new OfTotalFraction(total, fraction);

        public static IFraction<IDistance>[] FromTotal(IDistance total, params Double[] fractions) =>
            fractions.Select(f => new OfTotalFraction(total, f)).ToArray();

        private record OfFraction(IDistance Value, Double Fraction) : IFraction<IDistance>
        {
            private Lazy<IDistance> _LazyTotal = new Lazy<IDistance>(() => Value / Fraction);

            public IDistance Total => _LazyTotal.Value;
        }

        private record OfTotalFraction(IDistance Total, Double Fraction) : IFraction<IDistance>
        {
            private Lazy<IDistance> _LazyValue = new Lazy<IDistance>(() => Total * Fraction);

            public IDistance Value => _LazyValue.Value;
        }

        private record OfTotalValue(IDistance Total, IDistance Value) : IFraction<IDistance>
        {
            private Lazy<Double> _LazyValue = new Lazy<Double>(() => Value / Total);

            public Double Fraction => _LazyValue.Value;
        }
    }

    public record BoundedValue<T>(T Value, T Total) : IBoundedValue<T>;
}
