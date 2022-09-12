using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using System;

namespace RaceDirector.Pipeline.Games.ACC;

internal class TelemetryConverter
{
    internal GameTelemetry Transform(ref Contrib.Data.Shared sharedData)
    {
        if (sharedData.Static.SmVersion.Length == 0)
            return GameTelemetry.Empty;
        var smVersionMajor = Int32.Parse(sharedData.Static.SmVersion.Split('.')[0]);
        if (smVersionMajor != Contrib.Constant.SmVersionMajor)
            throw new ArgumentException("Incompatible major version");
        return new GameTelemetry(
            GameState: ToGameState(ref sharedData),
            UsingVR: null, // TODO Check if it can be inferred from process parameters
            Event: ToEvent(ref sharedData),
            Session: ToSession(ref sharedData),
            Vehicles: ToVehicles(ref sharedData),
            FocusedVehicle: ToFocusedVehicle(ref sharedData),
            Player: ToPlayer(ref sharedData)
        );
    }

    private static GameState ToGameState(ref Contrib.Data.Shared sharedData)
    {
        return sharedData.Graphic.Status switch
        {
            Contrib.Constant.Status.Off => GameState.Replay, // recorded replay
            Contrib.Constant.Status.Replay => GameState.Replay, // in-game replay
            Contrib.Constant.Status.Live when sharedData.Physics.WaterTemp == 0 => GameState.Menu, // in-game menu
            Contrib.Constant.Status.Live => GameState.Driving,
            Contrib.Constant.Status.Pause => GameState.Paused, // single player game paused
            _ => throw new ArgumentException("Unknown game state")
        };
    }

    private static Event? ToEvent(ref Contrib.Data.Shared sharedData)
    {
        return new Event(
            TrackLayout: new TrackLayout
            (
                // Sector length can be inferred from TrackData.TrackMeters (UDP)
                // and SPageFileGraphic.NormalizedCarPosition (MM). Sectors in SM
                // are considered 1/3 of the total length, but that is incorrect.
                // Will have to record them on track and save them in config.
                SectorsEnd: Array.Empty<IFraction<IDistance>>() // TODO
            ),
            FuelRate: sharedData.Static.AidFuelRate
        );
    }

    private static Session? ToSession(ref Contrib.Data.Shared sharedData)
    {
        var maybeSessionType = ToSessionType(sharedData.Graphic.Session);
        if (maybeSessionType is null)
            return null;

        return new Session
        (
            Type: maybeSessionType.Value,
            // It might be inferred from global flags, timers, etc.
            Phase: SessionPhase.Unknown, // TODO
            // Practice: timed (in race) or unlimited (SessionTimeLeft -1)
            // Hotlap: unlimited
            // HotstintSuperpole: 2 laps
            // Hotstint: timed
            // Qualify: timed
            // Race: timed
            Length: new RaceDuration.LapsDuration(10, null), // TODO
            Requirements: new SessionRequirements // TODO
            (
                0, MandatoryPitRequirements.None,
                new Interval<IPitWindowBoundary>
                (
                    new RaceDuration.LapsDuration(Laps: Convert.ToUInt32(0), EstimatedTime: null),
                    new RaceDuration.LapsDuration(Laps: Convert.ToUInt32(0), EstimatedTime: null)
                )
            ),
            PitSpeedLimit: ISpeed.FromMPS(0), // TODO
            PitLaneOpen: true, // TODO
            ElapsedTime: TimeSpan.Zero, // TODO
            TimeRemaining: TimeSpan.Zero, // TODO
            WaitTime: TimeSpan.Zero, // TODO
            StartLights: new StartLights // TODO
            (
                Color: LightColor.Green,
                Lit: new BoundedValue<uint>(0, 4)
            ),
            BestLap: null, // TODO
            BestSectors: null, // TODO
            Flags: new SessionFlags // TODO
            (
                Track: TrackFlags.None,
                Sectors: new SectorFlags[0],
                Leader: LeaderFlags.None
            )
        );
    }

    private static SessionType? ToSessionType(Contrib.Constant.SessionType session) => session switch
    {
        Contrib.Constant.SessionType.Practice => SessionType.Practice,
        Contrib.Constant.SessionType.Qualify => SessionType.Qualify,
        Contrib.Constant.SessionType.Race => SessionType.Race,
        Contrib.Constant.SessionType.Hotlap => SessionType.HotLap,
        Contrib.Constant.SessionType.Timeattack => SessionType.TimeAttack,
        Contrib.Constant.SessionType.Drift => SessionType.Drift,
        Contrib.Constant.SessionType.Drag => SessionType.Drag,
        Contrib.Constant.SessionType.Hotstint => SessionType.HotStint,
        Contrib.Constant.SessionType.HotstintSuperpole => SessionType.HotStintSuperPole,
        _ => null
    };

    private static Vehicle[] ToVehicles(ref Contrib.Data.Shared sharedData)
    {
        return Array.Empty<Vehicle>();
    }

    private static Vehicle? ToVehicle(ref Contrib.Data.Shared sharedData)
    {
        return new Vehicle
        (
            Id: 0, // TODO // TODO
            ClassPerformanceIndex: -1, // TODO
            RacingStatus: IRacingStatus.Unknown, // TODO
            EngineType: EngineType.Unknown, // TODO
            ControlType: ControlType.LocalPlayer, // TODO
            Position: 0, // TODO
            PositionClass: 0, // TODO
            GapAhead: null, // TODO
            GapBehind: null, // TODO
            CompletedLaps: 0, // TODO
            LapValid: LapValidState.Unknown, // TODO
            CurrentLapTime: null, // TODO
            PreviousLapTime: null, // TODO
            BestLapTime: null, // TODO
            BestSectors: null, // TODO
            CurrentLapDistance: DistanceFraction.Of(IDistance.FromM(1), 0), // TODO
            Location: new Vector3<IDistance> // TODO
            (
                X: IDistance.FromM(0),
                Y: IDistance.FromM(0),
                Z: IDistance.FromM(0)
            ),
            Orientation: null, // TODO
            Speed: ISpeed.FromMPS(0), // TODO
            CurrentDriver: new Driver // TODO
            (
                Name: ""
            ),
            Pit: new VehiclePit // TODO
            (
                StopsDone: 0,
                MandatoryStopsDone: 0,
                PitLanePhase: null,
                PitLaneTime: null,
                PitStallTime: null
            ),
            Penalties: Array.Empty<Penalty>(), // TODO
            Flags: new VehicleFlags // TODO
            (
                Green: null,
                Blue: null,
                Yellow: null,
                White: null,
                Checkered: null,
                Black: null,
                BlackWhite: null
            ),
            Inputs: null // TODO
        );
    }

    private static Vehicle? ToFocusedVehicle(ref Contrib.Data.Shared sharedData)
    {
        return new Vehicle
        (
            Id: 0, // TODO // TODO
            ClassPerformanceIndex: -1, // TODO
            RacingStatus: IRacingStatus.Unknown, // TODO
            EngineType: EngineType.Unknown, // TODO
            ControlType: ControlType.LocalPlayer, // TODO
            Position: 0, // TODO
            PositionClass: 0, // TODO
            GapAhead: null, // TODO
            GapBehind: null, // TODO
            CompletedLaps: 0, // TODO
            LapValid: LapValidState.Unknown, // TODO
            CurrentLapTime: null, // TODO
            PreviousLapTime : null, // TODO
            BestLapTime : null, // TODO
            BestSectors : null, // TODO
            CurrentLapDistance: DistanceFraction.Of(IDistance.FromM(1), 0), // TODO
            Location: new Vector3<IDistance> // TODO
            (
                X: IDistance.FromM(0),
                Y: IDistance.FromM(0),
                Z: IDistance.FromM(0)
            ),
            Orientation: null, // TODO
            Speed: ISpeed.FromMPS(0), // TODO
            CurrentDriver: new Driver // TODO
            (
                Name:  ""
            ),
            Pit: new VehiclePit // TODO
            (
                StopsDone: 0,
                MandatoryStopsDone: 0,
                PitLanePhase: null,
                PitLaneTime: null,
                PitStallTime: null
            ),
            Penalties: Array.Empty<Penalty>(), // TODO
            Flags: new VehicleFlags // TODO
            (
                Green : null,
                Blue : null,
                Yellow : null,
                White : null,
                Checkered : null,
                Black : null,
                BlackWhite : null
            ),
            Inputs: null // TODO
        );
    }

    private static Player? ToPlayer(ref Contrib.Data.Shared sharedData)
    {
        // TODO return null if player not driving
        return new Player
        (
            RawInputs: new RawInputs // TODO
            (
                Throttle: 0,
                Brake: 0,
                Clutch: 0,
                Steering: 0,
                SteerWheelRange: IAngle.FromDeg(0)
            ),
            DrivingAids: new DrivingAids // TODO
            (
                Abs: null,
                Tc: null,
                Esp: null,
                Countersteer: null,
                Cornering: null
            ),
            VehicleSettings: new VehicleSettings // TODO
            (
                EngineMap: null,
                EngineBrakeReduction: null
            ),
            VehicleDamage: new VehicleDamage // TODO
            (
                AerodynamicsPercent: 0,
                EnginePercent: 0,
                SuspensionPercent: 0,
                TransmissionPercent: 0
            ),
            Tires: Array.Empty<Tire[]>(),
            Fuel: new Fuel
            (
                Max: ICapacity.FromL(0), // TODO
                Left: ICapacity.FromL(sharedData.Physics.Fuel),
                PerLap: null // TODO
            ),
            Engine: new Engine // TODO
            (
                Speed: IAngularSpeed.FromRevPS(0),
                UpshiftSpeed: IAngularSpeed.FromRevPS(0),
                MaxSpeed: IAngularSpeed.FromRevPS(0)
            ),
            CgLocation: new Vector3<IDistance> // TODO
            (
                X: IDistance.FromM(0),
                Y: IDistance.FromM(0),
                Z: IDistance.FromM(0)
            ),
            Orientation: new Orientation // TODO
            (
                Yaw: IAngle.FromDeg(0),
                Pitch: IAngle.FromDeg(0),
                Roll: IAngle.FromDeg(0)
            ),
            LocalAcceleration: new Vector3<IAcceleration> // TODO
            (
                X: IAcceleration.FromMPS2(0),
                Y: IAcceleration.FromMPS2(0),
                Z: IAcceleration.FromMPS2(0)
            ),
            ClassBestLap: null,
            ClassBestSectors: null, // TODO
            PersonalBestSectors: null, // TODO
            PersonalBestDelta: null, // TODO
            Drs: null, // TODO
            PushToPass: null, // TODO
            PitStop: PlayerPitStop.None,
            Warnings: new PlayerWarnings // TODO
            (
                IncidentPoints: null,
                BlueFlagWarnings: null,
                GiveBackPositions: 0
            ),
            OvertakeAllowed: null, // TODO
            PitMenu: new PitMenu
            (
                FocusedItem: PitMenuFocusedItem.Unavailable,
                SelectedItems: 0,
                FuelToAdd: ICapacity.FromL(sharedData.Graphic.MfdFuelToAdd)
            )
        );
    }
}