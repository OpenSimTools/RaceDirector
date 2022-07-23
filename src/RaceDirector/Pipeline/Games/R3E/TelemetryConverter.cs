using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceDirector.Pipeline.Games.R3E;

internal class TelemetryConverter
{
    private const uint MaxLights = 5;

    // TODO what about passing the previous telemetry to the transform method?
    private readonly StatefulAid<Aid> _statefulAbs = StatefulAid.Generic();
    private readonly StatefulAid<TractionControl> _statefulTc = StatefulAid.Tc();
    private readonly StatefulAid<Aid> _statefulEsp = StatefulAid.Generic();
    private readonly StatefulAid<Aid> _statefulCountersteer = StatefulAid.Generic();
    private readonly StatefulAid<Aid> _statefulCornering = StatefulAid.Generic();
    private Interval<IPitWindowBoundary>? _pitWindowState;

    internal GameTelemetry Transform(ref Contrib.Data.Shared sharedData)
    {
        if (sharedData.VersionMajor != Contrib.Constant.VersionMajor)
            throw new ArgumentException("Incompatible major version");
        return new GameTelemetry(
            GameState: ToGameState(ref sharedData),
            UsingVR: sharedData.GameUsingVr > 0,
            Event: ToEvent(ref sharedData),
            Session: ToSession(ref sharedData),
            Vehicles: ToVehicles(ref sharedData),
            FocusedVehicle: ToFocusedVehicle(ref sharedData),
            Player: ToPlayer(ref sharedData)
        );
    }

    private static GameState ToGameState(ref Contrib.Data.Shared sharedData)
    {
        if (sharedData.GamePaused > 0)
            return GameState.Paused;
        if (sharedData.GameInMenus > 0)
            return GameState.Menu;
        if (sharedData.GameInReplay > 0)
            return GameState.Replay;
        return GameState.Driving; // TODO It should be in menu if no session
    }

    private Event? ToEvent(ref Contrib.Data.Shared sharedData)
    {
        var track = ToTrackLayout(ref sharedData);
        if (track == null)
            return null;
        return new Event(
            TrackLayout: track,
            FuelRate: sharedData.FuelUseActive >= 0 ? sharedData.FuelUseActive : 0
        );
    }

    private TrackLayout? ToTrackLayout(ref Contrib.Data.Shared sharedData)
    {
        if (sharedData.LayoutLength < 0)
            return null;
        var layoutLength = IDistance.FromM(sharedData.LayoutLength);
        var sectors = ValuesPerSector(ref sharedData.SectorStartFactors,
            i => DistanceFraction.FromTotal(layoutLength, i));
        return new TrackLayout(
            SectorsEnd: sectors
        );
    }

    private Session? ToSession(ref Contrib.Data.Shared sharedData)
    {
        var maybeSessionType = ToSessionType(ref sharedData);
        var maybeSessionPhase = ToSessionPhase(ref sharedData);
        if (maybeSessionType is null || maybeSessionPhase is null)
            return null;
        var maybeSessionLength = ToCurrentSessionLength(ref sharedData);
        var sessionRequirements = ToSessionRequirements(ref sharedData);
        var (elapsedTime, timeRemaining, waitTime) = RemainingTime(ref sharedData);

        return new Session
        (
            Type: maybeSessionType.Value,
            Phase: maybeSessionPhase.Value,
            Length: maybeSessionLength,
            Requirements: sessionRequirements,
            PitSpeedLimit: ISpeed.FromMPS(sharedData.SessionPitSpeedLimit),
            PitLaneOpen: sharedData.PitWindowStatus > 0,
            ElapsedTime: elapsedTime,
            TimeRemaining: timeRemaining,
            WaitTime: waitTime,
            StartLights: ToStartLights(sharedData.StartLights),
            BestLap: null, // TODO
            BestSectors: null, // TODO
            Flags: new SessionFlags // TODO
            (
                Track: TrackFlags.None,
                Sectors: Array.Empty<SectorFlags>(),
                Leader: LeaderFlags.None
            )
        );
    }

    private static SessionType? ToSessionType(ref Contrib.Data.Shared sharedData) =>
        sharedData.SessionType switch
        {
            Contrib.Constant.Session.Practice => SessionType.Practice,
            Contrib.Constant.Session.Qualify => SessionType.Qualify,
            Contrib.Constant.Session.Warmup => SessionType.Warmup,
            Contrib.Constant.Session.Race => SessionType.Race,
            _ => null
        };

    private static SessionPhase? ToSessionPhase(ref Contrib.Data.Shared sharedData) =>
        sharedData.SessionPhase switch
        {
            Contrib.Constant.SessionPhase.Garage => SessionPhase.Garage,
            Contrib.Constant.SessionPhase.Gridwalk => SessionPhase.GridWalk,
            Contrib.Constant.SessionPhase.Formation => SessionPhase.Formation,
            Contrib.Constant.SessionPhase.Countdown => SessionPhase.Countdown,
            Contrib.Constant.SessionPhase.Green => SessionPhase.Started,
            Contrib.Constant.SessionPhase.Checkered => SessionPhase.Over,
            _ => null
        };

    private ISessionDuration? ToCurrentSessionLength(ref Contrib.Data.Shared sharedData) =>
        ToSessionLength(sharedData.SessionTimeDuration, sharedData.NumberOfLaps);

    private ISessionDuration? ToSessionLength(double secondsOrNegative, int lapsOrNegative)
    {
        if (lapsOrNegative >= 0)
        {
            if (secondsOrNegative >= 0)
                return new RaceDuration.TimePlusLapsDuration
                (
                    Time: TimeSpan.FromSeconds(secondsOrNegative),
                    ExtraLaps: Convert.ToUInt32(lapsOrNegative),
                    EstimatedLaps: null // TODO
                );
            else
                return new RaceDuration.LapsDuration
                (
                    Laps: Convert.ToUInt32(lapsOrNegative),
                    EstimatedTime: null // TODO
                );
        }
        else
        {
            if (secondsOrNegative >= 0)
                return new RaceDuration.TimeDuration
                (
                    Time: TimeSpan.FromSeconds(secondsOrNegative),
                    EstimatedLaps: null // TODO
                );
            else
                return null;
        }
    }

    private SessionRequirements ToSessionRequirements(ref Contrib.Data.Shared sharedData)
    {
        var currentPitWindow = ToPitWindow(ref sharedData);

        // R3E removes the pit window when after it ends but we don't want that!
        var pitWindowIsCorrect = sharedData.PitWindowStatus switch
        {
            Contrib.Constant.PitWindow.Unavailable => true,
            Contrib.Constant.PitWindow.Disabled => true,
            Contrib.Constant.PitWindow.Open => true,
            _ => false // In other cases, the pit window might be wrong
        };

        // Keep the  previous pit window if null and unsure
        if (currentPitWindow != null || pitWindowIsCorrect)
            _pitWindowState = currentPitWindow;

        // R3E only supports a single mandatory pit stop
        var mandatoryPitStops = _pitWindowState is null ? 0u : 1u;

        return new SessionRequirements
        (
            MandatoryPitStops: mandatoryPitStops,
            MandatoryPitRequirements: 0, // TODO
            PitWindow: _pitWindowState
        );
    }

    private static (TimeSpan?, TimeSpan?, TimeSpan?) RemainingTime(ref Contrib.Data.Shared sharedData)
    {
        var timeRemaining = sharedData.SessionTimeRemaining;
        var sessionTimeLength = sharedData.SessionTimeDuration;

        if (sharedData.SessionPhase == Contrib.Constant.SessionPhase.Checkered)
            // TODO infer elapsed time after checkered flag by storing GameSimulationTime at t0
            return (null, TimeSpan.Zero, TimeSpan.FromSeconds(timeRemaining));

        if (sessionTimeLength < 0 || timeRemaining < 0)
            return (null, null, null);

        return (TimeSpan.FromSeconds(sessionTimeLength - timeRemaining), TimeSpan.FromSeconds(timeRemaining), null);
    }

    private static Interval<IPitWindowBoundary>? ToPitWindow(ref Contrib.Data.Shared sharedData)
    {
        if (sharedData.PitWindowStart <= 0 || sharedData.PitWindowEnd <= 0)
            return null;
        else
            return sharedData.SessionLengthFormat switch
            {
                Contrib.Constant.SessionLengthFormat.LapBased =>
                    new Interval<IPitWindowBoundary>
                    (
                        new RaceDuration.LapsDuration
                        (
                            Laps: Convert.ToUInt32(sharedData.PitWindowStart),
                            EstimatedTime: null // TODO
                        ),
                        new RaceDuration.LapsDuration
                        (
                            Laps: Convert.ToUInt32(sharedData.PitWindowEnd),
                            EstimatedTime: null // TODO
                        )
                    ),
                _ =>
                    new Interval<IPitWindowBoundary>
                    (
                        new RaceDuration.TimeDuration
                        (
                            Time: TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowStart)),
                            EstimatedLaps: null // TODO
                        ),
                        new RaceDuration.TimeDuration
                        (
                            Time: TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowEnd)),
                            EstimatedLaps: null // TODO
                        )
                    ),
            };
    }

    private static StartLights? ToStartLights(int startLights)
    {
        if (startLights < 0)
            return null;
        if (startLights > MaxLights)
            return new StartLights
            (
                Color: LightColor.Green,
                Lit: new BoundedValue<uint>(MaxLights, MaxLights)
            );
        return new StartLights
        (
            Color: LightColor.Red,
            Lit: new BoundedValue<uint>((uint) startLights, MaxLights)
        );
    }

    private Vehicle[] ToVehicles(ref Contrib.Data.Shared sharedData)
    {
        if (sharedData.NumCars <= 0)
            return Array.Empty<Vehicle>();
        var vehicles = new Vehicle[sharedData.NumCars];
        for (var i = 0; i < sharedData.NumCars; i++)
        {
            vehicles[i] = ToVehicle(ref sharedData.DriverData[i], ref sharedData);
        }

        return vehicles;
    }

    private Vehicle ToVehicle(ref Contrib.Data.DriverData driverData, ref Contrib.Data.Shared sharedData)
    {
        return new Vehicle
        (
            Id: SafeUInt32(driverData.DriverInfo.SlotId),
            ClassPerformanceIndex: driverData.DriverInfo.ClassPerformanceIndex,
            RacingStatus: ToRacingStatus(driverData.FinishStatus),
            EngineType: EngineType.Unknown, // TODO
            ControlType: ControlType.Replay, // TODO
            Position: 42, // TODO
            PositionClass: SafeUInt32(driverData.PlaceClass),
            GapAhead: TimeSpan.FromSeconds(driverData.TimeDeltaFront), // It can be negative!
            GapBehind: TimeSpan.FromSeconds(driverData.TimeDeltaBehind), // It can be negative!
            CompletedLaps: SafeUInt32(driverData.CompletedLaps),
            LapValid: ToLapValid(driverData.CurrentLapValid),
            CurrentLapTime: null, // TODO
            PreviousLapTime: null, // TODO
            BestLapTime: new LapTime
            (
                Overall: TimeSpan.FromSeconds(42), // TODO
                Sectors: new Sectors
                (
                    Individual: Array.Empty<TimeSpan>(), // TODO infer from cumulative
                    Cumulative: ValuesPerSector(ref driverData.SectorTimeBestSelf, i => TimeSpan.FromSeconds(i))
                )
            ),
            BestSectors: null, // TODO
            CurrentLapDistance: DistanceFraction.FromTotal(IDistance.FromM(sharedData.LayoutLength),
                IDistance.FromM(driverData.LapDistance)),
            Location: Vector3(ref driverData.Position, i => IDistance.FromM(i)),
            Orientation: null, // TODO
            Speed: ISpeed.FromMPS(42), // TODO
            CurrentDriver: new Driver
            (
                Name: FromNullTerminatedByteArray(driverData.DriverInfo.Name)
            ),
            Pit: new VehiclePit
            (
                StopsDone: SafeUInt32(driverData.NumPitstops),
                MandatoryStopsDone: ToMandatoryStopsDone(driverData.PitStopStatus),
                PitLanePhase: null, // TODO
                PitLaneTime: null, // TODO
                PitStallTime: null // TODO
            ),
            Penalties: Array.Empty<Penalty>(), // TODO
            Flags: new VehicleFlags
            (
                Green: null,
                Blue: null,
                Yellow: null,
                White: null,
                Checkered: null,
                Black: null,
                BlackWhite: null
            ),
            // IFocusedVehicle only
            Inputs: null
        );
    }

    private Vehicle? ToFocusedVehicle(ref Contrib.Data.Shared sharedData)
    {
        var currentSlotId = sharedData.VehicleInfo.SlotId;
        if (currentSlotId < 0)
            return null;
        var driverDataIndex = Array.FindIndex(sharedData.DriverData, dd => dd.DriverInfo.SlotId == currentSlotId);
        if (driverDataIndex < 0)
            return null;
        var currentDriverData = sharedData.DriverData[driverDataIndex];
        var driverName = FromNullTerminatedByteArray(sharedData.PlayerName);

        return new Vehicle
        (
            Id: SafeUInt32(sharedData.VehicleInfo.SlotId),
            ClassPerformanceIndex: sharedData.VehicleInfo.ClassPerformanceIndex,
            RacingStatus: ToRacingStatus(currentDriverData.FinishStatus), // TODO
            EngineType: ToEngineType(ref sharedData),
            ControlType: ToControlType(ref sharedData),
            Position: 42, // TODO
            PositionClass: SafeUInt32(sharedData.PositionClass),
            GapAhead: null, // TODO
            GapBehind: null, // TODO
            CompletedLaps: SafeUInt32(currentDriverData.CompletedLaps),
            LapValid: ToLapValid(sharedData.CurrentLapValid),
            CurrentLapTime: new LapTime
            (
                Overall: TimeSpan.FromSeconds(sharedData.LapTimeCurrentSelf),
                Sectors: new Sectors
                (
                    Individual: Array.Empty<TimeSpan>(), // TODO infer from cumulative
                    Cumulative: ValuesPerSector(ref sharedData.SectorTimesCurrentSelf, i => TimeSpan.FromSeconds(i))
                )
            ),
            PreviousLapTime: null, // TODO
            BestLapTime: new LapTime
            (
                Overall: TimeSpan.FromSeconds(sharedData.LapTimeBestSelf),
                Sectors: new Sectors
                (
                    Individual: Array.Empty<TimeSpan>(), // TODO infer from cumulative
                    Cumulative: ValuesPerSector(ref sharedData.SectorTimesBestSelf, i => TimeSpan.FromSeconds(i))
                )
            ),
            BestSectors: null, // TODO
            CurrentLapDistance: DistanceFraction.Of(IDistance.FromM(sharedData.LapDistance),
                sharedData.LapDistanceFraction),
            Location: Vector3(ref sharedData.CarCgLocation, i => IDistance.FromM(i)),
            Orientation: ToOrientation(ref sharedData.CarOrientation, i => IAngle.FromRad(i)),
            Speed: ISpeed.FromMPS(sharedData.CarSpeed),
            CurrentDriver: new Driver
            (
                Name: driverName
            ),
            Pit: new VehiclePit
            (
                StopsDone: SafeUInt32(currentDriverData.NumPitstops),
                MandatoryStopsDone: ToMandatoryStopsDone(currentDriverData.PitStopStatus),
                PitLanePhase: ToPitLanePhase(ref sharedData),
                PitLaneTime: TimeSpan.FromSeconds(sharedData.PitTotalDuration),
                PitStallTime: TimeSpan.FromSeconds(sharedData.PitElapsedTime)
            ),
            Penalties: ToPenalties(sharedData.Penalties),
            Flags: ToVehicleFlags(sharedData.Flags),
            // IFocusedVehicle only
            Inputs: new Inputs
            (
                Throttle: sharedData.Throttle,
                Brake: sharedData.Brake,
                Clutch: sharedData.Clutch
            )
        );
    }

    private EngineType ToEngineType(ref Contrib.Data.Shared sharedData) =>
        sharedData.VehicleInfo.EngineType switch
        {
            Contrib.Constant.EngineType.Combustion => EngineType.Combustion,
            Contrib.Constant.EngineType.Electric => EngineType.Electric,
            Contrib.Constant.EngineType.Hybrid => EngineType.Hybrid,
            _ => EngineType.Unknown
        };

    private ControlType ToControlType(ref Contrib.Data.Shared sharedData) =>
        sharedData.ControlType switch
        {
            Contrib.Constant.Control.Player => ControlType.LocalPlayer,
            Contrib.Constant.Control.AI => ControlType.AI,
            Contrib.Constant.Control.Remote => ControlType.RemotePlayer,
            Contrib.Constant.Control.Replay => ControlType.Replay,
            _ => ControlType.LocalPlayer // TODO
        };

    private LapValidState ToLapValid(int currentLapValid) => currentLapValid switch
    {
        0 => LapValidState.Invalid,
        1 => LapValidState.Valid,
        _ => LapValidState.Unknown
    };

    private PitLanePhase? ToPitLanePhase(ref Contrib.Data.Shared sharedData) =>
        sharedData.PitState switch
        {
            2 => PitLanePhase.Entered,
            3 => PitLanePhase.Stopped,
            4 => PitLanePhase.Exiting,
            _ => null
        };

    private uint ToMandatoryStopsDone(Contrib.Constant.PitStopStatus pitStopStatus) =>
        (Contrib.Constant.PitStopStatus.Served == pitStopStatus) ? 1u : 0u;

    private static Penalty[] ToPenalties(Contrib.Data.CutTrackPenalties cutTrackPenalties)
    {
        var penalties = new List<Penalty>();
        if (cutTrackPenalties.DriveThrough > 0)
            penalties.Add(new Penalty(PenaltyType.DriveThrough, PenaltyReason.Unknown));
        if (cutTrackPenalties.StopAndGo > 0)
            penalties.Add(new Penalty(PenaltyType.StopAndGo10, PenaltyReason.Unknown));
        if (cutTrackPenalties.PitStop > 0)
            penalties.Add(new Penalty(PenaltyType.PitStop, PenaltyReason.Unknown));
        if (cutTrackPenalties.TimeDeduction > 0)
            penalties.Add(new Penalty(PenaltyType.TimeDeduction, PenaltyReason.Unknown));
        if (cutTrackPenalties.SlowDown > 0)
            penalties.Add(new Penalty(PenaltyType.SlowDown, PenaltyReason.Unknown));
        return penalties.ToArray();
    }

    private static VehicleFlags ToVehicleFlags(Contrib.Data.Flags flags)
    {
        // TODO return -1 or 0 depending on what state the game is on
        // only black and checquered available during replay
        // not sure when in menus
        return new VehicleFlags
        (
            Green: flags.Green > 0 ? new GreenFlag(IVehicleFlags.GreenReason.RaceStart) : null,
            Blue: flags.Blue > 0 ? new BlueFlag(IVehicleFlags.BlueReason.Unknown) : null,
            Yellow: flags.Yellow > 0 ? new YellowFlag(IVehicleFlags.YellowReason.Unknown) : null,
            White: flags.White > 0 ? new WhiteFlag(IVehicleFlags.WhiteReason.SlowCarAhead) : null,
            Checkered: flags.Checkered > 0 ? new Flag() : null,
            Black: flags.Black > 0 ? new Flag() : null,
            BlackWhite: flags.BlackAndWhite switch
            {
                -1 => null,
                0 => null,
                1 => new BlackWhiteFlag(IVehicleFlags.BlackWhiteReason.IgnoredBlueFlags),
                2 => new BlackWhiteFlag(IVehicleFlags.BlackWhiteReason.IgnoredBlueFlags),
                3 => new BlackWhiteFlag(IVehicleFlags.BlackWhiteReason.WrongWay),
                4 => new BlackWhiteFlag(IVehicleFlags.BlackWhiteReason.Cutting),
                _ => new BlackWhiteFlag(IVehicleFlags.BlackWhiteReason.Unknown),
            }
        );
    }

    private static IRacingStatus ToRacingStatus(Contrib.Constant.FinishStatus finishStatus) =>
        finishStatus switch
        {
            Contrib.Constant.FinishStatus.None => IRacingStatus.Racing,
            Contrib.Constant.FinishStatus.Finished => IRacingStatus.Finished,
            Contrib.Constant.FinishStatus.DNF => IRacingStatus.DNF,
            Contrib.Constant.FinishStatus.DNQ => IRacingStatus.DNQ,
            Contrib.Constant.FinishStatus.DNS => IRacingStatus.DNS,
            Contrib.Constant.FinishStatus.DQ => new IRacingStatus.DQ(IRacingStatus.DQReason.Unknown),
            _ => IRacingStatus.Unknown,
        };


    private Player? ToPlayer(ref Contrib.Data.Shared sharedData)
    {
        // TODO some of this is present in replay/monitor
        if (sharedData.VehicleInfo.UserId < 0)
            return null;
        return new Player
        (
            RawInputs: new RawInputs
            (
                Throttle: sharedData.ThrottleRaw,
                Brake: sharedData.BrakeRaw,
                Clutch: sharedData.ClutchRaw,
                Steering: sharedData.SteerInputRaw,
                SteerWheelRange: IAngle.FromDeg(sharedData.SteerWheelRangeDegrees)
            ),
            DrivingAids: new DrivingAids
            (
                Abs: _statefulAbs.Update(sharedData.AidSettings.Abs),
                Tc: _statefulTc.Update(sharedData.AidSettings.Tc),
                Esp: _statefulEsp.Update(sharedData.AidSettings.Esp),
                Countersteer: _statefulCountersteer.Update(sharedData.AidSettings.Countersteer),
                Cornering: _statefulCornering.Update(sharedData.AidSettings.Cornering)
            ),
            VehicleSettings: new VehicleSettings
            (
                EngineMap: null, // TODO
                EngineBrakeReduction: null // TODO
            ),
            VehicleDamage: new VehicleDamage
            (
                AerodynamicsPercent: sharedData.CarDamage.Aerodynamics,
                EnginePercent: sharedData.CarDamage.Engine,
                SuspensionPercent: sharedData.CarDamage.Suspension,
                TransmissionPercent: sharedData.CarDamage.Transmission
            ),
            Tires: ToTires(ref sharedData),
            Fuel: new Fuel
            (
                Max: sharedData.FuelCapacity,
                Left: sharedData.FuelLeft,
                PerLap: sharedData.FuelPerLap
            ),
            Engine: new Engine
            (
                Speed: IAngularSpeed.FromRadPS(sharedData.EngineRps),
                UpshiftSpeed: IAngularSpeed.FromRadPS(sharedData.UpshiftRps),
                MaxSpeed: IAngularSpeed.FromRadPS(sharedData.MaxEngineRps)
            ),
            CgLocation: Vector3(ref sharedData.Player.Position, IDistance.FromM),
            Orientation: new Orientation
            (
                Yaw: IAngle.FromDeg(0.0), // TODO
                Pitch: IAngle.FromDeg(0.0), // TODO
                Roll: IAngle.FromDeg(0.0) // TODO
            ),
            LocalAcceleration: Vector3(ref sharedData.Player.LocalAcceleration, IAcceleration.FromMPS2),
            ClassBestLap: null, // TODO
            ClassBestSectors: new
                Sectors // TODO are these the best sectors of the class leader or the best class sectors???
                (
                    Individual: ValuesPerSector(ref sharedData.BestIndividualSectorTimeLeaderClass,
                        i => TimeSpan.FromSeconds(i)),
                    Cumulative: Array.Empty<TimeSpan>() // TODO
                ),
            PersonalBestSectors: new Sectors
            (
                Individual: ValuesPerSector(ref sharedData.BestIndividualSectorTimeSelf,
                    i => TimeSpan.FromSeconds(i)),
                Cumulative: Array.Empty<TimeSpan>() // TODO
            ),
            PersonalBestDelta: TimeSpan.FromSeconds(sharedData.TimeDeltaBestSelf),
            Drs: ToDrs(ref sharedData),
            PushToPass: ToPtp(ref sharedData),
            PitStop: ToPlayerPitStop(ref sharedData),
            Warnings: new PlayerWarnings
            (
                IncidentPoints: null,
                BlueFlagWarnings: ToBlueFlagWarnings(sharedData.Flags.BlackAndWhite),
                GiveBackPositions: PositiveOrZero(sharedData.Flags.YellowPositionsGained)
            ),
            OvertakeAllowed: NullableBoolean(sharedData.Flags.YellowOvertake)
        );
    }

    private static PlayerPitStop ToPlayerPitStop(ref Contrib.Data.Shared sharedData)
    {
        var playerPitStop = PlayerPitStop.None;
        if (sharedData.PitState == 1)
            playerPitStop |= PlayerPitStop.Requested;
        foreach (var (pitActionFlag, playerPitStopFlag) in PitActionFlags)
            if ((sharedData.PitAction & pitActionFlag) != 0)
                playerPitStop |= playerPitStopFlag;
        return playerPitStop;
    }

    private static readonly (int, PlayerPitStop)[] PitActionFlags =
    {
        (1, PlayerPitStop.Preparing),
        (2, PlayerPitStop.ServingPenalty),
        (4, PlayerPitStop.DriverChange),
        (8, PlayerPitStop.Refuelling),
        (16, PlayerPitStop.ChangeFrontTires),
        (32, PlayerPitStop.ChangeRearTires),
        (64, PlayerPitStop.RepairBody),
        (128, PlayerPitStop.RepairFrontWing),
        (256, PlayerPitStop.RepairRearWing),
        (512, PlayerPitStop.RepairSuspension)
    };

    private static Orientation ToOrientation<T>(ref Contrib.Data.Orientation<T> value, Func<T, IAngle> f) =>
        new(Yaw: f(value.Yaw), Pitch: f(value.Pitch), Roll: f(value.Roll));

    private ActivationToggled? ToDrs(ref Contrib.Data.Shared sharedData)
    {
        var drs = sharedData.Drs;
        if (drs.Equipped <= 0)
            return null;
        return new ActivationToggled
        (
            Available: drs.Available > 0,
            Engaged: drs.Engaged > 0,
            ActivationsLeft: new BoundedValue<uint>(
                Value: SafeUInt32(drs.NumActivationsLeft),
                Total: SafeUInt32(sharedData.DrsNumActivationsTotal)
            )
        );
    }

    private static WaitTimeToggled? ToPtp(ref Contrib.Data.Shared sharedData)
    {
        var ptp = sharedData.PushToPass;
        if (ptp.Available < 0)
            return null;
        return new WaitTimeToggled
        (
            Available: ptp.Available > 0,
            Engaged: ptp.Engaged > 0,
            ActivationsLeft: new BoundedValue<uint>
            (
                Value: SafeUInt32(ptp.AmountLeft),
                Total: SafeUInt32(sharedData.PtpNumActivationsTotal)
            ),
            EngagedTimeLeft: TimeSpan.FromSeconds(ptp.EngagedTimeLeft),
            WaitTimeLeft: TimeSpan.FromSeconds(ptp.WaitTimeLeft)
        );
    }

    private static Tire[][] ToTires(ref Contrib.Data.Shared sharedData)
    {
        return new[]
        {
            new[]
            {
                ToTire(ref sharedData, new ITireExtractor.FrontLeft()),
                ToTire(ref sharedData, new ITireExtractor.FrontRight())
            },
            new[]
            {
                ToTire(ref sharedData, new ITireExtractor.RearLeft()),
                ToTire(ref sharedData, new ITireExtractor.RearRight())
            }
        };
    }

    private static Tire ToTire(ref Contrib.Data.Shared sharedData, ITireExtractor extract)
    {
        var tireTemps = extract.CurrentTire(ref sharedData.TireTemp);
        var brakeTemps = extract.CurrentTire(ref sharedData.BrakeTemp);
        return new Tire(
            Dirt: extract.CurrentTire(ref sharedData.TireDirt),
            Grip: extract.CurrentTire(ref sharedData.TireGrip),
            Wear: extract.CurrentTire(ref sharedData.TireWear),
            Temperatures: new TemperaturesMatrix
            (
                CurrentTemperatures: new[]
                {
                    new[]
                    {
                        ITemperature.FromC(tireTemps.CurrentTemp.Left),
                        ITemperature.FromC(tireTemps.CurrentTemp.Center),
                        ITemperature.FromC(tireTemps.CurrentTemp.Right)
                    }
                },
                OptimalTemperature: ITemperature.FromC(tireTemps.OptimalTemp),
                ColdTemperature: ITemperature.FromC(tireTemps.ColdTemp),
                HotTemperature: ITemperature.FromC(tireTemps.HotTemp)
            ),
            BrakeTemperatures: new TemperaturesSingle
            (
                CurrentTemperature: ITemperature.FromC(brakeTemps.CurrentTemp),
                OptimalTemperature: ITemperature.FromC(brakeTemps.OptimalTemp),
                ColdTemperature: ITemperature.FromC(brakeTemps.ColdTemp),
                HotTemperature: ITemperature.FromC(brakeTemps.HotTemp)
            )
        );
    }

    private interface ITireExtractor
    {
        T CurrentTire<T>(ref Contrib.Data.TireData<T> outer);

        class FrontLeft : ITireExtractor
        {
            T ITireExtractor.CurrentTire<T>(ref Contrib.Data.TireData<T> outer) => outer.FrontLeft;
        }

        class FrontRight : ITireExtractor
        {
            T ITireExtractor.CurrentTire<T>(ref Contrib.Data.TireData<T> outer) => outer.FrontRight;
        }

        class RearLeft : ITireExtractor
        {
            T ITireExtractor.CurrentTire<T>(ref Contrib.Data.TireData<T> outer) => outer.RearLeft;
        }

        class RearRight : ITireExtractor
        {
            T ITireExtractor.CurrentTire<T>(ref Contrib.Data.TireData<T> outer) => outer.RearRight;
        }
    }

    private static IBoundedValue<uint> ToBlueFlagWarnings(int blackAndWhite)
    {
        uint blueWarnings = blackAndWhite switch
        {
            1 => 1,
            2 => 2,
            _ => 0
        };
        return new BoundedValue<uint>(blueWarnings, 2);
    }

    private static uint PositiveOrZero(int value) => value > 0 ? (uint) value : 0;

    private static string FromNullTerminatedByteArray(byte[] nullTerminated)
    {
        var nullIndex = Array.IndexOf<byte>(nullTerminated, 0);
        if (nullIndex < 0)
            nullIndex = nullTerminated.Length;
        return Encoding.UTF8.GetString(nullTerminated, 0, nullIndex);
    }

    private static Vector3<TO> Vector3<TI, TO>(ref Contrib.Data.Vector3<TI> value, Func<TI, TO> f) =>
        new(X: f(value.X), Y: f(value.Y), Z: f(value.Z));

    private static TO[] ValuesPerSector<TI, TO>(ref Contrib.Data.Sectors<TI> value, Func<TI, TO> f) =>
        new[] {f(value.Sector1), f(value.Sector2), f(value.Sector3)};

    private static TO[] ValuesPerSector<TI, TO>(ref Contrib.Data.SectorStarts<TI> value, Func<TI, TO> f) =>
        new[] {f(value.Sector1), f(value.Sector2), f(value.Sector3)};

    private static uint SafeUInt32(int i, uint defaultValue = 0)
    {
        return i < 0 ? defaultValue : (uint) i;
    }

    private static bool? NullableBoolean(int i)
    {
        return i switch
        {
            < 0 => null,
            > 0 => true,
            _ => false
        };
    }
}

/// <summary>
/// Keeps track of the previous aid value for when its activated value hides it.
/// </summary>
public class StatefulAid<T> where T : Aid
{
    private T? _current;
    private readonly Func<uint, T> _constructor;

    public StatefulAid(Func<uint, T> constructor)
    {
        _constructor = constructor;
    }

    public T? Update(int newLevel)
    {
        switch (newLevel)
        {
            case < 0:
                _current = null;
                break;
            case 5:
                if (_current != null)
                    _current = _current with {Active = true};
                break;
            default:
                _current = _constructor((uint) newLevel);
                break;
        }

        return _current;
    }
}

public static class StatefulAid
{
    public static StatefulAid<Aid> Generic() => new(level => new Aid(level, false));

    public static StatefulAid<TractionControl> Tc() => new(level => new TractionControl(level, false, null));
}