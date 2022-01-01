using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceDirector.Pipeline.Games.R3E
{
    static class Telemetry
    {
        private static readonly UInt32 MaxLights = 5;

        public static GameTelemetry Transform(Contrib.Data.Shared sharedData)
        {
            if (sharedData.VersionMajor != (Int32)Contrib.Constant.VersionMajor.R3E_VERSION_MAJOR)
                throw new ArgumentException("Incompatible major version");
            return new GameTelemetry(
                GameState: GameState(sharedData),
                UsingVR: sharedData.GameUsingVr > 0,
                Event: Event(sharedData),
                Session: Session(sharedData),
                Vehicles: Vehicles(sharedData),
                FocusedVehicle: FocusedVehicle(sharedData),
                Player: Player(sharedData)
            );
        }

        private static Pipeline.Telemetry.V0.GameState GameState(Contrib.Data.Shared sharedData)
        {
            if (sharedData.GameInMenus > 0)
                return Pipeline.Telemetry.V0.GameState.Menu;
            if (sharedData.GameInReplay > 0)
                return Pipeline.Telemetry.V0.GameState.Replay;
            return Pipeline.Telemetry.V0.GameState.Driving;
        }

        private static Event? Event(Contrib.Data.Shared sharedData)
        {
            var track = Track(sharedData);
            if (track == null)
                return null;
            return new Event(
                Track: track,
                FuelRate: sharedData.FuelUseActive >= 0 ? sharedData.FuelUseActive : 0
            );
        }

        private static TrackLayout? Track(Contrib.Data.Shared sharedData)
        {
            if (sharedData.LayoutLength < 0)
                return null;
            var layoutLength = IDistance.FromM(sharedData.LayoutLength);
            var sectors = ValuesPerSector(sharedData.SectorStartFactors, i => DistanceFraction.FromTotal(layoutLength, i));
            return new TrackLayout(
                SectorsEnd: sectors
            );
        }

        private static Session? Session(Contrib.Data.Shared sharedData)
        {
            var maybeSessionType = SessionType(sharedData);
            var maybeSessionPhase = SessionPhase(sharedData);
            if (maybeSessionType is null || maybeSessionPhase is null)
                return null;
            var maybeSessionLength = SessionLength(sharedData);
            var sessionRequirements = SessionRequirements(sharedData);

            return new Session
            (
                Type: maybeSessionType.Value,
                Phase: maybeSessionPhase.Value,
                Length: maybeSessionLength,
                Requirements: sessionRequirements,
                PitSpeedLimit: ISpeed.FromMPS(sharedData.SessionPitSpeedLimit),
                ElapsedTime: TimeSpan.FromSeconds(sharedData.SessionTimeDuration - sharedData.SessionTimeRemaining), // TODO overtime? add both?
                StartLights: StartLights(sharedData.StartLights),
                BestLap: null, // TODO
                BestSectors: null, // TODO
                Flags: new SessionFlags( // TODO
                    Track: Pipeline.Telemetry.V0.TrackFlags.None,
                    Sectors: new Pipeline.Telemetry.V0.SectorFlags[0],
                    Leader: Pipeline.Telemetry.V0.LeaderFlags.None
                )
            );
        }

        private static Pipeline.Telemetry.V0.SessionType? SessionType(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.Session)sharedData.SessionType switch
            {
                Contrib.Constant.Session.Practice => Pipeline.Telemetry.V0.SessionType.Practice,
                Contrib.Constant.Session.Qualify => Pipeline.Telemetry.V0.SessionType.Qualify,
                Contrib.Constant.Session.Warmup => Pipeline.Telemetry.V0.SessionType.Warmup,
                Contrib.Constant.Session.Race => Pipeline.Telemetry.V0.SessionType.Race,
                _ => null
            };

        private static Pipeline.Telemetry.V0.SessionPhase? SessionPhase(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.SessionPhase)sharedData.SessionPhase switch
            {
                Contrib.Constant.SessionPhase.Garage => Pipeline.Telemetry.V0.SessionPhase.Garage,
                Contrib.Constant.SessionPhase.Gridwalk => Pipeline.Telemetry.V0.SessionPhase.Gridwalk,
                Contrib.Constant.SessionPhase.Formation => Pipeline.Telemetry.V0.SessionPhase.Formation,
                Contrib.Constant.SessionPhase.Countdown => Pipeline.Telemetry.V0.SessionPhase.Countdown,
                Contrib.Constant.SessionPhase.Green => Pipeline.Telemetry.V0.SessionPhase.Started,
                Contrib.Constant.SessionPhase.Checkered => Pipeline.Telemetry.V0.SessionPhase.Over,
                _ => null
            };

        private static Pipeline.Telemetry.V0.ISessionDuration? SessionLength(Contrib.Data.Shared sharedData)
        {
            // TODO Create Event.SessionsLength and get the SessionIteration-1 element from it
            var (laps, minutes) = sharedData.SessionIteration switch
            {
                1 => (sharedData.RaceSessionLaps.Race1, sharedData.RaceSessionMinutes.Race1),
                2 => (sharedData.RaceSessionLaps.Race2, sharedData.RaceSessionMinutes.Race2),
                3 => (sharedData.RaceSessionLaps.Race3, sharedData.RaceSessionMinutes.Race3),
                _ => (-1, -1)
            }; // -1 no race, 0 not used, >0 used
            if (laps > 0)
            {
                if (minutes > 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimePlusLapsDuration
                    (
                        Time: TimeSpan.FromMinutes(minutes),
                        ExtraLaps: Convert.ToUInt32(laps),
                        EstimatedLaps: null // TODO
                    );
                else
                    return new Pipeline.Telemetry.V0.RaceDuration.LapsDuration
                    (
                        Laps: Convert.ToUInt32(laps),
                        EstimatedTime: null // TODO
                    );
            }
            else
            {
                if (minutes > 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimeDuration
                    (
                        Time: TimeSpan.FromMinutes(minutes),
                        EstimatedLaps: null // TODO
                    );
                else
                    return null;
            }
        }

        private static SessionRequirements SessionRequirements(Contrib.Data.Shared sharedData)
        {
            if (sharedData.PitWindowStart <= 0 || sharedData.PitWindowEnd <= 0)
                return new SessionRequirements(
                    MandatoryPitStops: 0,
                    MandatoryPitRequirements: 0,
                    PitWindow: null
                );

            var window = (Contrib.Constant.SessionLengthFormat)sharedData.SessionLengthFormat switch
            {
                Contrib.Constant.SessionLengthFormat.LapBased =>
                    new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                        new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                            Laps: Convert.ToUInt32(sharedData.PitWindowStart),
                            EstimatedTime: null // TODO
                        ),
                        new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                            Laps: Convert.ToUInt32(sharedData.PitWindowStart),
                            EstimatedTime: null // TODO
                        )
                    ),
                _ =>
                    new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                        new Pipeline.Telemetry.V0.RaceDuration.TimeDuration(
                            Time: TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowStart)),
                            EstimatedLaps: null // TODO
                        ),
                        new Pipeline.Telemetry.V0.RaceDuration.TimeDuration(
                            Time: TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowEnd)),
                            EstimatedLaps: null // TODO
                        )
                    ),
            };
            return new SessionRequirements(
                MandatoryPitStops: 1, // R3E only supports a single mandatory pit stop
                MandatoryPitRequirements: 0, // TODO
                PitWindow: window
            );
        }

        private static StartLights? StartLights(Int32 startLights)
        {
            if (startLights < 0)
                return null;
            if (startLights > MaxLights)
                return new StartLights(
                    Colour: Pipeline.Telemetry.V0.LightColour.Green,
                    Lit: new BoundedValue<UInt32>(MaxLights, MaxLights)
                );
            return new StartLights(
                Colour: Pipeline.Telemetry.V0.LightColour.Red,
                Lit: new BoundedValue<UInt32>((UInt32)startLights, MaxLights)
            );
        }

        private static Vehicle[] Vehicles(Contrib.Data.Shared sharedData)
        {
            if (sharedData.NumCars <= 0)
                return Array.Empty<Vehicle>();
            var vehicles = new Vehicle[sharedData.NumCars];
            for (var i = 0; i < sharedData.NumCars; i++)
            {
                vehicles[i] = Vehicle(sharedData.DriverData[i], sharedData);
            }
            return vehicles;
        }
        
        private static Vehicle Vehicle(Contrib.Data.DriverData driverData, Contrib.Data.Shared sharedData)
        {
            return new Vehicle(
                Id: SafeUInt32(driverData.DriverInfo.SlotId),
                ClassPerformanceIndex: driverData.DriverInfo.ClassPerformanceIndex,
                RacingStatus: Pipeline.Telemetry.V0.IRacingStatus.Unknown, // TODO
                EngineType: Pipeline.Telemetry.V0.EngineType.Unknown, // TODO
                ControlType: Pipeline.Telemetry.V0.ControlType.Replay, // TODO
                Position: 42, // TODO
                PositionClass: SafeUInt32(driverData.PlaceClass),
                GapAhead: NullableTimeSpan(driverData.TimeDeltaFront),   // TODO ********* check when close to a lapped car
                GapBehind: NullableTimeSpan(driverData.TimeDeltaBehind), // TODO ********* check when being lapped
                CompletedLaps: SafeUInt32(driverData.CompletedLaps),
                CurrentLapValid: driverData.CurrentLapValid > 0,
                CurrentLapTime: null, // TODO
                PreviousLapTime: null, // TODO
                BestLapTime: new LapTime(
                    Overall: TimeSpan.FromSeconds(42), // TODO
                    Sectors: new Sectors(
                        Individual: new TimeSpan[0], // TODO infer from cumulative
                        Cumulative: ValuesPerSector(driverData.SectorTimeBestSelf, i => TimeSpan.FromSeconds(i))
                    )
                ),
                BestSectors: null, // TODO
                CurrentLapDistance: DistanceFraction.FromTotal(IDistance.FromM(sharedData.LayoutLength), IDistance.FromM(driverData.LapDistance)),
                Location: Vector3(driverData.Position, i => IDistance.FromM(i)),
                Orientation: null, // TODO
                Speed: ISpeed.FromMPS(42), // TODO
                CurrentDriver: new Driver(
                    Name: FromNullTerminatedByteArray(driverData.DriverInfo.Name) ?? "TODO"
                ),
                Pit: new VehiclePit(
                    StopsDone: 42, // TODO
                    MandatoryStopsDone: 42, // TODO
                    PitLaneState: null, // TODO
                    PitLaneTime: null, // TODO
                    PitStallTime: null // TODO
                ),
                Penalties: new Penalty[0], // TODO
                // IFocusedVehicle only
                Inputs: null,
                Flags: new VehicleFlags(
                    Green: null,
                    Blue: null,
                    Yellow: null,
                    White: null,
                    Chequered: null,
                    Black: null,
                    BlackWhite: null
                )
            );
        }

        private static Vehicle? FocusedVehicle(Contrib.Data.Shared sharedData)
        {
            Int32 currentSlotId = sharedData.VehicleInfo.SlotId;
            if (currentSlotId < 0)
                return null;
            var driverDataIndex = Array.FindIndex(sharedData.DriverData, dd => dd.DriverInfo.SlotId == currentSlotId);
            if (driverDataIndex < 0)
                return null;
            var currentDriverData = sharedData.DriverData[driverDataIndex];
            var driverName = FromNullTerminatedByteArray(sharedData.PlayerName);
            if (driverName == null)
                return null;

            return new Vehicle(
                Id: SafeUInt32(sharedData.VehicleInfo.SlotId),
                ClassPerformanceIndex: sharedData.VehicleInfo.ClassPerformanceIndex,
                RacingStatus: Pipeline.Telemetry.V0.IRacingStatus.Unknown, // TODO
                EngineType: EngineType(sharedData),
                ControlType: ControlType(sharedData),
                Position: 42, // TODO
                PositionClass: SafeUInt32(sharedData.PositionClass),
                GapAhead: null, // TODO
                GapBehind: null, // TODO
                CompletedLaps: SafeUInt32(currentDriverData.CompletedLaps),
                CurrentLapValid: sharedData.CurrentLapValid > 0, // of course different from CurrentLapValid for the current vehicle!
                CurrentLapTime: new LapTime(
                    Overall: TimeSpan.FromSeconds(sharedData.LapTimeCurrentSelf),
                    Sectors: new Sectors
                    (
                        Individual: new TimeSpan[0], // TODO infer from cumulative
                        Cumulative: ValuesPerSector(sharedData.SectorTimesCurrentSelf, i => TimeSpan.FromSeconds(i))
                    )
                ),
                PreviousLapTime: null, // TODO
                BestLapTime: new LapTime(
                    Overall: TimeSpan.FromSeconds(sharedData.LapTimeBestSelf),
                    Sectors: new Sectors
                    (
                        Individual: new TimeSpan[0], // TODO infer from cumulative
                        Cumulative: ValuesPerSector(sharedData.SectorTimesBestSelf, i => TimeSpan.FromSeconds(i))
                    )
                ),
                BestSectors: null, // TODO
                CurrentLapDistance: DistanceFraction.Of(IDistance.FromM(sharedData.LapDistance), sharedData.LapDistanceFraction),
                Location: Vector3(sharedData.CarCgLocation, i => IDistance.FromM(i)),
                Orientation: Orientation(sharedData.CarOrientation, i => IAngle.FromRad(i)),
                Speed: ISpeed.FromMPS(sharedData.CarSpeed),
                CurrentDriver: new Driver
                (
                    Name: driverName
                ),
                Pit: new VehiclePit
                (
                    StopsDone: SafeUInt32(currentDriverData.NumPitstops),
                    MandatoryStopsDone: ((Int32)Contrib.Constant.PitWindow.Completed == currentDriverData.PitStopStatus) ? 1u : 0u,
                    PitLaneState: PitLaneState(sharedData),
                    PitLaneTime: TimeSpan.FromSeconds(sharedData.PitTotalDuration),
                    PitStallTime: TimeSpan.FromSeconds(sharedData.PitElapsedTime)
                ),
                Penalties: Penalties(sharedData.Penalties),
                // IFocusedVehicle only
                Inputs: new Inputs
                (
                    Throttle: sharedData.Throttle,
                    Brake: sharedData.Brake,
                    Clutch: sharedData.Clutch
                ),
                Flags: VehicleFlags(sharedData.Flags)
            );
        }

        private static Pipeline.Telemetry.V0.EngineType EngineType(Contrib.Data.Shared sharedData) =>
            sharedData.VehicleInfo.EngineType switch
            {
                (int)Contrib.Constant.EngineType.COMBUSTION => Pipeline.Telemetry.V0.EngineType.Combustion,
                (int)Contrib.Constant.EngineType.ELECTRIC => Pipeline.Telemetry.V0.EngineType.Electric,
                (int)Contrib.Constant.EngineType.HYBRID => Pipeline.Telemetry.V0.EngineType.Hybrid,
                _ => Pipeline.Telemetry.V0.EngineType.Unknown
            };

        private static Pipeline.Telemetry.V0.ControlType ControlType(Contrib.Data.Shared sharedData) =>
            sharedData.ControlType switch
            {
                (int)Contrib.Constant.Control.Player => Pipeline.Telemetry.V0.ControlType.LocalPlayer,
                (int)Contrib.Constant.Control.AI => Pipeline.Telemetry.V0.ControlType.AI,
                (int)Contrib.Constant.Control.Remote => Pipeline.Telemetry.V0.ControlType.RemotePlayer,
                (int)Contrib.Constant.Control.Replay => Pipeline.Telemetry.V0.ControlType.Replay,
                _ => Pipeline.Telemetry.V0.ControlType.LocalPlayer // TODO
            };

        private static Pipeline.Telemetry.V0.PitLaneState? PitLaneState(Contrib.Data.Shared sharedData) =>
            sharedData.PitState switch
            {
                2 => Pipeline.Telemetry.V0.PitLaneState.Entered,
                3 => Pipeline.Telemetry.V0.PitLaneState.Stopped,
                4 => Pipeline.Telemetry.V0.PitLaneState.Exiting,
                _ => null
            };

        private static Penalty[] Penalties(Contrib.Data.CutTrackPenalties cutTrackPenalties)
        {
            var penalties = new List<Penalty>();
            if (cutTrackPenalties.DriveThrough > 0)
                penalties.Add(new Penalty(Pipeline.Telemetry.V0.PenaltyType.DriveThrough, Pipeline.Telemetry.V0.PenaltyReason.Unknown));
            if (cutTrackPenalties.StopAndGo > 0)
                penalties.Add(new Penalty(Pipeline.Telemetry.V0.PenaltyType.StopAndGo10, Pipeline.Telemetry.V0.PenaltyReason.Unknown));
            if (cutTrackPenalties.PitStop > 0)
                penalties.Add(new Penalty(Pipeline.Telemetry.V0.PenaltyType.PitStop, Pipeline.Telemetry.V0.PenaltyReason.Unknown));
            if (cutTrackPenalties.TimeDeduction > 0)
                penalties.Add(new Penalty(Pipeline.Telemetry.V0.PenaltyType.TimeDeduction, Pipeline.Telemetry.V0.PenaltyReason.Unknown));
            if (cutTrackPenalties.SlowDown > 0)
                penalties.Add(new Penalty(Pipeline.Telemetry.V0.PenaltyType.SlowDown, Pipeline.Telemetry.V0.PenaltyReason.Unknown));
            return penalties.ToArray();
        }

        private static VehicleFlags VehicleFlags(Contrib.Data.Flags flags)
        {
            // TODO return -1 or 0 depending on what state the game is on
            // only black and checquered available during replay
            // not sure when in menus
            return new VehicleFlags
            (
                Green: flags.Green > 0 ? new GreenFlag(Pipeline.Telemetry.V0.IVehicleFlags.GreenReason.RaceStart) : null,
                Blue: flags.Blue > 0 ? new BlueFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlueReason.Unknown) : null,
                Yellow: flags.Yellow > 0 ? new YellowFlag(Pipeline.Telemetry.V0.IVehicleFlags.YellowReason.Unknown, flags.YellowOvertake > 0) : null,
                White: flags.White > 0 ? new WhiteFlag(Pipeline.Telemetry.V0.IVehicleFlags.WhiteReason.SlowCarAhead) : null,
                Chequered: flags.Checkered > 0 ? new Flag() : null,
                Black: flags.Black > 0 ? new Flag() : null,
                BlackWhite: flags.BlackAndWhite switch {
                    -1 => null,
                    0 => null,
                    1 => new BlackWhiteFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlackWhiteReason.IgnoredBlueFlags),
                    2 => new BlackWhiteFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlackWhiteReason.IgnoredBlueFlags),
                    3 => new BlackWhiteFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlackWhiteReason.WrongWay),
                    4 => new BlackWhiteFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlackWhiteReason.Cutting),
                    _ => new BlackWhiteFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlackWhiteReason.Unknown),
                }
            );
        }


        private static Player? Player(Contrib.Data.Shared sharedData)
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
                    Abs: Aid(sharedData.AidSettings.Abs),
                    Tc: TractionControl(sharedData.AidSettings.Tc),
                    Esp: Aid(sharedData.AidSettings.Esp),
                    Countersteer: Aid(sharedData.AidSettings.Countersteer),
                    Cornering: Aid(sharedData.AidSettings.Cornering)
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
                Tyres: Tyres(sharedData),
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
                CgLocation: Vector3(sharedData.Player.Position, i => IDistance.FromM(i)),
                Orientation: new Orientation
                (
                    Yaw: IAngle.FromDeg(0.0), // TODO
                    Pitch: IAngle.FromDeg(0.0), // TODO
                    Roll: IAngle.FromDeg(0.0) // TODO
                ),
                LocalAcceleration: Vector3(sharedData.Player.LocalAcceleration, i => IAcceleration.FromMPS2(i)),
                LapValid: Pipeline.Telemetry.V0.LapValidState.Valid, // TODO and move to Vehicle!
                ClassBestLap: null, // TODO
                ClassBestSectors: new Sectors // TODO are these the best sectors of the class leader or the best class sectors???
                (
                    Individual: ValuesPerSector(sharedData.BestIndividualSectorTimeLeaderClass, i => TimeSpan.FromSeconds(i)),
                    Cumulative: new TimeSpan[0] // TODO
                ),
                PersonalBestSectors: new Sectors
                (
                    Individual: ValuesPerSector(sharedData.BestIndividualSectorTimeSelf, i => TimeSpan.FromSeconds(i)),
                    Cumulative: new TimeSpan[0] // TODO
                ),
                PersonalBestDelta: TimeSpan.FromSeconds(sharedData.TimeDeltaBestSelf),
                Drs: ActivationToggled(sharedData.Drs, sharedData),
                PushToPass: WaitTimeToggled(sharedData.PushToPass, sharedData),
                PitStop: PlayerPitStop(sharedData),
                Warnings: new PlayerWarnings
                (
                    IncidentPoints: null,
                    BlueFlagWarnings: BlueFlagWarnings(sharedData.Flags.BlackAndWhite),
                    GiveBackPositions: PositiveOrZero(sharedData.Flags.YellowPositionsGained)
                )
            );
        }

        private static Pipeline.Telemetry.V0.PlayerPitStop PlayerPitStop(Contrib.Data.Shared sharedData)
        {
            var playerPitStop = Pipeline.Telemetry.V0.PlayerPitStop.None;
            if (sharedData.PitState == 1)
                playerPitStop |= Pipeline.Telemetry.V0.PlayerPitStop.Requested;
            foreach (var (pitActionFlag, playerPitstopFlag) in pitActionFlags)
                if ((sharedData.PitAction & pitActionFlag) != 0) playerPitStop |= playerPitstopFlag;
            return playerPitStop;
        }

        private static readonly (Int32, Pipeline.Telemetry.V0.PlayerPitStop)[] pitActionFlags = {
                (  1, Pipeline.Telemetry.V0.PlayerPitStop.Preparing),
                (  2, Pipeline.Telemetry.V0.PlayerPitStop.ServingPenalty),
                (  4, Pipeline.Telemetry.V0.PlayerPitStop.DriverChange),
                (  8, Pipeline.Telemetry.V0.PlayerPitStop.Refuelling),
                ( 16, Pipeline.Telemetry.V0.PlayerPitStop.ChangeFrontTyres),
                ( 32, Pipeline.Telemetry.V0.PlayerPitStop.ChangeRearTyres),
                ( 64, Pipeline.Telemetry.V0.PlayerPitStop.RepairBody),
                (128, Pipeline.Telemetry.V0.PlayerPitStop.RepairFrontWing),
                (256, Pipeline.Telemetry.V0.PlayerPitStop.RepairRearWing),
                (512, Pipeline.Telemetry.V0.PlayerPitStop.RepairSuspension)
            };

        private static Orientation Orientation<I>(Contrib.Data.Orientation<I> value, Func<I, IAngle> f) =>
            new Orientation(Yaw: f(value.Yaw), Pitch: f(value.Pitch), Roll: f(value.Roll));

        private static Aid? Aid(Int32 i) {
            if (i < 0)
                return null;
            if (i == 5)
                return new Aid(42, true); // TODO store last seen state
            return new Aid((UInt32)i, false);
        }

        private static TractionControl? TractionControl(Int32 i)
        {
            var aid = Aid(i);
            if (aid == null)
                return null;
            return new TractionControl(aid.Level, aid.Active, null);
        }

        private static ActivationToggled? ActivationToggled(Contrib.Data.DRS drs, Contrib.Data.Shared sharedData)
        {
            if (drs.Equipped <= 0)
                return null;
            return new ActivationToggled
                (
                Available: drs.Available > 0,
                Engaged: drs.Engaged > 0,
                ActivationsLeft: new BoundedValue<UInt32>(
                    Value: SafeUInt32(drs.NumActivationsLeft),
                    Total: SafeUInt32(sharedData.DrsNumActivationsTotal)
                )
            );
        }
        
        private static WaitTimeToggled? WaitTimeToggled(Contrib.Data.PushToPass ptp, Contrib.Data.Shared sharedData)
        {
            if (ptp.Available < 0)
                return null;
            return new WaitTimeToggled
                (
                Available: ptp.Available > 0,
                Engaged: ptp.Engaged > 0,
                ActivationsLeft: new BoundedValue<UInt32>
                (
                    Value: SafeUInt32(ptp.AmountLeft),
                    Total: SafeUInt32(sharedData.PtpNumActivationsTotal)
                ),
                EngagedTimeLeft: TimeSpan.FromSeconds(ptp.EngagedTimeLeft),
                WaitTimeLeft: TimeSpan.FromSeconds(ptp.WaitTimeLeft)
            );
        }

        private static Tyre[][] Tyres(Contrib.Data.Shared sharedData)
        {
            return new Tyre[][]
            {
                new Tyre[] {
                    Tyre(sharedData, new ITyreExtractor.FrontLeft()),
                    Tyre(sharedData, new ITyreExtractor.FrontRight())
                },
                new Tyre[] {
                    Tyre(sharedData, new ITyreExtractor.RearLeft()),
                    Tyre(sharedData, new ITyreExtractor.RearRight())
                }
            };
        }

        private static Tyre Tyre(Contrib.Data.Shared sharedData, ITyreExtractor extract)
        {
            var tyreTemps = extract.CurrentTyre(sharedData.TireTemp);
            var brakeTemps = extract.CurrentTyre(sharedData.BrakeTemp);
            return new Tyre(
                Dirt: extract.CurrentTyre(sharedData.TireDirt),
                Grip: extract.CurrentTyre(sharedData.TireGrip),
                Wear: extract.CurrentTyre(sharedData.TireWear),
                Temperatures: new TemperaturesMatrix
                (
                    CurrentTemperatures: new ITemperature[][] { new ITemperature[] {
                        ITemperature.FromC(tyreTemps.CurrentTemp.Left),
                        ITemperature.FromC(tyreTemps.CurrentTemp.Center),
                        ITemperature.FromC(tyreTemps.CurrentTemp.Right)
                    }},
                    OptimalTemperature: ITemperature.FromC(tyreTemps.OptimalTemp),
                    ColdTemperature: ITemperature.FromC(tyreTemps.ColdTemp),
                    HotTemperature: ITemperature.FromC(tyreTemps.HotTemp)
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

        private interface ITyreExtractor
        {
            T CurrentTyre<T>(Contrib.Data.TireData<T> outer);

            class FrontLeft : ITyreExtractor
            {
                T ITyreExtractor.CurrentTyre<T>(Contrib.Data.TireData<T> outer) => outer.FrontLeft;
            }

            class FrontRight : ITyreExtractor
            {
                T ITyreExtractor.CurrentTyre<T>(Contrib.Data.TireData<T> outer) => outer.FrontRight;
            }

            class RearLeft : ITyreExtractor
            {
                T ITyreExtractor.CurrentTyre<T>(Contrib.Data.TireData<T> outer) => outer.RearLeft;
            }

            class RearRight : ITyreExtractor
            {
                T ITyreExtractor.CurrentTyre<T>(Contrib.Data.TireData<T> outer) => outer.RearRight;
            }
        }

        private static IBoundedValue<UInt32>? BlueFlagWarnings(Int32 blackAndWhite)
        {
            UInt32 blueWarnings = blackAndWhite switch {
                1 => 1,
                2 => 2,
                _ => 0
            };
            return new BoundedValue<UInt32>(blueWarnings, 2);
        }

        private static UInt32 PositiveOrZero(Int32 value) => value > 0 ? (UInt32)value : 0;

        private static String? FromNullTerminatedByteArray(byte[] nullTerminated)
        {
            var nullIndex = Array.IndexOf<byte>(nullTerminated, 0);
            if (nullIndex <= 0)
                return null;
            return Encoding.UTF8.GetString(nullTerminated, 0, nullIndex);
        }

        private static Vector3<O> Vector3<I, O>(Contrib.Data.Vector3<I> value, Func<I, O> f) =>
            new Vector3<O>(X: f(value.X), Y: f(value.Y), Z: f(value.Z));

        private static O[] ValuesPerSector<I, O>(Contrib.Data.Sectors<I> value, Func<I, O> f) =>
            new O[] { f(value.Sector1), f(value.Sector2), f(value.Sector3) };

        private static O[] ValuesPerSector<I, O>(Contrib.Data.SectorStarts<I> value, Func<I, O> f) =>
            new O[] { f(value.Sector1), f(value.Sector2), f(value.Sector3) };

        private static UInt32 SafeUInt32(Int32 i, UInt32 defaultValue = 0)
        {
            if (i < 0)
                return defaultValue;
            else
                return (UInt32)i;
        }

        private static TimeSpan? NullableTimeSpan(Single value)
        {
            if (value < 0.0)
                return null;
            return TimeSpan.FromSeconds(value);
        }
    }
}
