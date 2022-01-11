using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceDirector.Pipeline.Games.R3E
{
    internal class Telemetry
    {
        private const UInt32 MaxLights = 5;

        // TODO what about passing the previous telemetry to the transform method?
        private StatefulAid<Aid> statefulAbs = StatefulAid.Generic();
        private StatefulAid<TractionControl> statefulTc = StatefulAid.Tc();
        private StatefulAid<Aid> statefulEsp = StatefulAid.Generic();
        private StatefulAid<Aid> statefulCountersteer = StatefulAid.Generic();
        private StatefulAid<Aid> statefulCornering = StatefulAid.Generic();
        private Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>? pitWindowState = null;

        internal GameTelemetry Transform(Contrib.Data.Shared sharedData)
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

        private Pipeline.Telemetry.V0.GameState GameState(Contrib.Data.Shared sharedData)
        {
            if (sharedData.GameInMenus > 0)
                return Pipeline.Telemetry.V0.GameState.Menu;
            if (sharedData.GameInReplay > 0)
                return Pipeline.Telemetry.V0.GameState.Replay;
            return Pipeline.Telemetry.V0.GameState.Driving;
        }

        private Event? Event(Contrib.Data.Shared sharedData)
        {
            var track = Track(sharedData);
            if (track == null)
                return null;
            return new Event(
                Track: track,
                FuelRate: sharedData.FuelUseActive >= 0 ? sharedData.FuelUseActive : 0
            );
        }

        private TrackLayout? Track(Contrib.Data.Shared sharedData)
        {
            if (sharedData.LayoutLength < 0)
                return null;
            var layoutLength = IDistance.FromM(sharedData.LayoutLength);
            var sectors = ValuesPerSector(sharedData.SectorStartFactors, i => DistanceFraction.FromTotal(layoutLength, i));
            return new TrackLayout(
                SectorsEnd: sectors
            );
        }

        private Session? Session(Contrib.Data.Shared sharedData)
        {
            var maybeSessionType = SessionType(sharedData);
            var maybeSessionPhase = SessionPhase(sharedData);
            if (maybeSessionType is null || maybeSessionPhase is null)
                return null;
            var maybeSessionLength = CurrentSessionLength(sharedData);
            var sessionRequirements = SessionRequirements(sharedData);
            var (elapsedTime, timeRemaining, waitTime) = RemainingTime(sharedData);

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

        private Pipeline.Telemetry.V0.SessionType? SessionType(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.Session)sharedData.SessionType switch
            {
                Contrib.Constant.Session.Practice => Pipeline.Telemetry.V0.SessionType.Practice,
                Contrib.Constant.Session.Qualify => Pipeline.Telemetry.V0.SessionType.Qualify,
                Contrib.Constant.Session.Warmup => Pipeline.Telemetry.V0.SessionType.Warmup,
                Contrib.Constant.Session.Race => Pipeline.Telemetry.V0.SessionType.Race,
                _ => null
            };

        private Pipeline.Telemetry.V0.SessionPhase? SessionPhase(Contrib.Data.Shared sharedData) =>
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

        private Pipeline.Telemetry.V0.ISessionDuration? CurrentSessionLength(Contrib.Data.Shared sharedData) =>
            SessionLength(sharedData.SessionTimeDuration, sharedData.NumberOfLaps);

        private Pipeline.Telemetry.V0.ISessionDuration? SessionLength(Double secondsOrNegative, Int32 lapsOrNegative)
        {
            if (lapsOrNegative >= 0)
            {
                if (secondsOrNegative >= 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimePlusLapsDuration
                    (
                        Time: TimeSpan.FromSeconds(secondsOrNegative),
                        ExtraLaps: Convert.ToUInt32(lapsOrNegative),
                        EstimatedLaps: null // TODO
                    );
                else
                    return new Pipeline.Telemetry.V0.RaceDuration.LapsDuration
                    (
                        Laps: Convert.ToUInt32(lapsOrNegative),
                        EstimatedTime: null // TODO
                    );
            }
            else
            {
                if (secondsOrNegative >= 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimeDuration
                    (
                        Time: TimeSpan.FromSeconds(secondsOrNegative),
                        EstimatedLaps: null // TODO
                    );
                else
                    return null;
            }
        }

        private SessionRequirements SessionRequirements(Contrib.Data.Shared sharedData)
        {
            var currentPitWindow = PitWindow(sharedData);

            // R3E removes the pit window when after it ends but we don't want that!
            var pitWindowIsCorrect = (Contrib.Constant.PitWindow)sharedData.PitWindowStatus switch {
                Contrib.Constant.PitWindow.Unavailable => true,
                Contrib.Constant.PitWindow.Disabled => true,
                Contrib.Constant.PitWindow.Open => true,
                _ => false // In other cases, the pit window might be wrong
            };

            // Keep the  previous pit window if null and unsure
            if (currentPitWindow != null || pitWindowIsCorrect)
                pitWindowState = currentPitWindow;

            // R3E only supports a single mandatory pit stop
            var mandatoryPitStops = pitWindowState is null ? 0u : 1u;

            return new SessionRequirements(
                MandatoryPitStops: mandatoryPitStops,
                MandatoryPitRequirements: 0, // TODO
                PitWindow: pitWindowState
            );
        }

        private (TimeSpan?, TimeSpan?, TimeSpan?) RemainingTime(Contrib.Data.Shared sharedData)
        {
            var timeRemaining = sharedData.SessionTimeRemaining;
            var sessionTimeLength = sharedData.SessionTimeDuration;

            if ((Contrib.Constant.SessionPhase)sharedData.SessionPhase == Contrib.Constant.SessionPhase.Checkered)
                // TODO infer elapsed time after chequered flag by storing GameSimulationTime at t0
                return (null, TimeSpan.Zero, TimeSpan.FromSeconds(timeRemaining));

            if (sessionTimeLength < 0 || timeRemaining < 0)
                return (null, null, null);

            return (TimeSpan.FromSeconds(sessionTimeLength - timeRemaining), TimeSpan.FromSeconds(timeRemaining), null);
        }

        private Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>? PitWindow(Contrib.Data.Shared sharedData)
        {
            if (sharedData.PitWindowStart <= 0 || sharedData.PitWindowEnd <= 0)
                return null;
            else
                return (Contrib.Constant.SessionLengthFormat)sharedData.SessionLengthFormat switch
                {
                    Contrib.Constant.SessionLengthFormat.LapBased =>
                        new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                            new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                                Laps: Convert.ToUInt32(sharedData.PitWindowStart),
                                EstimatedTime: null // TODO
                            ),
                            new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                                Laps: Convert.ToUInt32(sharedData.PitWindowEnd),
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
        }

        private StartLights? StartLights(Int32 startLights)
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

        private Vehicle[] Vehicles(Contrib.Data.Shared sharedData)
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
        
        private Vehicle Vehicle(Contrib.Data.DriverData driverData, Contrib.Data.Shared sharedData)
        {
            return new Vehicle(
                Id: SafeUInt32(driverData.DriverInfo.SlotId),
                ClassPerformanceIndex: driverData.DriverInfo.ClassPerformanceIndex,
                RacingStatus: RacingStatus((Contrib.Constant.FinishStatus)driverData.FinishStatus),
                EngineType: Pipeline.Telemetry.V0.EngineType.Unknown, // TODO
                ControlType: Pipeline.Telemetry.V0.ControlType.Replay, // TODO
                Position: 42, // TODO
                PositionClass: SafeUInt32(driverData.PlaceClass),
                GapAhead: TimeSpan.FromSeconds(driverData.TimeDeltaFront),   // It can be negative!
                GapBehind: TimeSpan.FromSeconds(driverData.TimeDeltaBehind), // It can be negative!
                CompletedLaps: SafeUInt32(driverData.CompletedLaps),
                LapValid: LapValid(driverData.CurrentLapValid),
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
                    StopsDone: SafeUInt32(driverData.NumPitstops),
                    MandatoryStopsDone: MandatoryStopsDone((Contrib.Constant.PitStopStatus)driverData.PitStopStatus),
                    PitLanePhase: null, // TODO
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

        private Vehicle? FocusedVehicle(Contrib.Data.Shared sharedData)
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
                RacingStatus: RacingStatus((Contrib.Constant.FinishStatus)currentDriverData.FinishStatus), // TODO
                EngineType: EngineType(sharedData),
                ControlType: ControlType(sharedData),
                Position: 42, // TODO
                PositionClass: SafeUInt32(sharedData.PositionClass),
                GapAhead: null, // TODO
                GapBehind: null, // TODO
                CompletedLaps: SafeUInt32(currentDriverData.CompletedLaps),
                LapValid: LapValid(sharedData.CurrentLapValid),
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
                    MandatoryStopsDone: MandatoryStopsDone((Contrib.Constant.PitStopStatus)currentDriverData.PitStopStatus),
                    PitLanePhase: PitLanePhase(sharedData),
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

        private Pipeline.Telemetry.V0.EngineType EngineType(Contrib.Data.Shared sharedData) =>
            sharedData.VehicleInfo.EngineType switch
            {
                (int)Contrib.Constant.EngineType.COMBUSTION => Pipeline.Telemetry.V0.EngineType.Combustion,
                (int)Contrib.Constant.EngineType.ELECTRIC => Pipeline.Telemetry.V0.EngineType.Electric,
                (int)Contrib.Constant.EngineType.HYBRID => Pipeline.Telemetry.V0.EngineType.Hybrid,
                _ => Pipeline.Telemetry.V0.EngineType.Unknown
            };

        private Pipeline.Telemetry.V0.ControlType ControlType(Contrib.Data.Shared sharedData) =>
            sharedData.ControlType switch
            {
                (int)Contrib.Constant.Control.Player => Pipeline.Telemetry.V0.ControlType.LocalPlayer,
                (int)Contrib.Constant.Control.AI => Pipeline.Telemetry.V0.ControlType.AI,
                (int)Contrib.Constant.Control.Remote => Pipeline.Telemetry.V0.ControlType.RemotePlayer,
                (int)Contrib.Constant.Control.Replay => Pipeline.Telemetry.V0.ControlType.Replay,
                _ => Pipeline.Telemetry.V0.ControlType.LocalPlayer // TODO
            };

        private Pipeline.Telemetry.V0.LapValidState LapValid(Int32 currentLapValid) => currentLapValid switch
        {
            0 => Pipeline.Telemetry.V0.LapValidState.Invalid,
            1 => Pipeline.Telemetry.V0.LapValidState.Valid,
            _ => Pipeline.Telemetry.V0.LapValidState.Unknown
        };

        private Pipeline.Telemetry.V0.PitLanePhase? PitLanePhase(Contrib.Data.Shared sharedData) =>
            sharedData.PitState switch
            {
                2 => Pipeline.Telemetry.V0.PitLanePhase.Entered,
                3 => Pipeline.Telemetry.V0.PitLanePhase.Stopped,
                4 => Pipeline.Telemetry.V0.PitLanePhase.Exiting,
                _ => null
            };

        private UInt32 MandatoryStopsDone(Contrib.Constant.PitStopStatus pitStopStatus) =>
            (Contrib.Constant.PitStopStatus.Served == pitStopStatus) ? 1u : 0u;

        private Penalty[] Penalties(Contrib.Data.CutTrackPenalties cutTrackPenalties)
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

        private VehicleFlags VehicleFlags(Contrib.Data.Flags flags)
        {
            // TODO return -1 or 0 depending on what state the game is on
            // only black and checquered available during replay
            // not sure when in menus
            return new VehicleFlags
            (
                Green: flags.Green > 0 ? new GreenFlag(Pipeline.Telemetry.V0.IVehicleFlags.GreenReason.RaceStart) : null,
                Blue: flags.Blue > 0 ? new BlueFlag(Pipeline.Telemetry.V0.IVehicleFlags.BlueReason.Unknown) : null,
                Yellow: flags.Yellow > 0 ? new YellowFlag(Pipeline.Telemetry.V0.IVehicleFlags.YellowReason.Unknown) : null,
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

        private Pipeline.Telemetry.V0.IRacingStatus RacingStatus(Contrib.Constant.FinishStatus finishStatus) =>
            finishStatus switch
            {
                Contrib.Constant.FinishStatus.None => Pipeline.Telemetry.V0.IRacingStatus.Racing,
                Contrib.Constant.FinishStatus.Finished => Pipeline.Telemetry.V0.IRacingStatus.Finished,
                Contrib.Constant.FinishStatus.DNF => Pipeline.Telemetry.V0.IRacingStatus.DNF,
                Contrib.Constant.FinishStatus.DNQ => Pipeline.Telemetry.V0.IRacingStatus.DNQ,
                Contrib.Constant.FinishStatus.DNS => Pipeline.Telemetry.V0.IRacingStatus.DNS,
                Contrib.Constant.FinishStatus.DQ => new Pipeline.Telemetry.V0.IRacingStatus.DQ(Pipeline.Telemetry.V0.IRacingStatus.DQReason.Unknown),
                _ => Pipeline.Telemetry.V0.IRacingStatus.Unknown,
            };


        private Player? Player(Contrib.Data.Shared sharedData)
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
                    Abs: statefulAbs.Update(sharedData.AidSettings.Abs),
                    Tc: statefulTc.Update(sharedData.AidSettings.Tc),
                    Esp: statefulEsp.Update(sharedData.AidSettings.Esp),
                    Countersteer: statefulCountersteer.Update(sharedData.AidSettings.Countersteer),
                    Cornering: statefulCornering.Update(sharedData.AidSettings.Cornering)
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
                ),
                OvertakeAllowed: NullableBoolean(sharedData.Flags.YellowOvertake)
            );
        }

        private Pipeline.Telemetry.V0.PlayerPitStop PlayerPitStop(Contrib.Data.Shared sharedData)
        {
            var playerPitStop = Pipeline.Telemetry.V0.PlayerPitStop.None;
            if (sharedData.PitState == 1)
                playerPitStop |= Pipeline.Telemetry.V0.PlayerPitStop.Requested;
            foreach (var (pitActionFlag, playerPitstopFlag) in pitActionFlags)
                if ((sharedData.PitAction & pitActionFlag) != 0) playerPitStop |= playerPitstopFlag;
            return playerPitStop;
        }

        private readonly (Int32, Pipeline.Telemetry.V0.PlayerPitStop)[] pitActionFlags = {
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

        private Orientation Orientation<I>(Contrib.Data.Orientation<I> value, Func<I, IAngle> f) =>
            new Orientation(Yaw: f(value.Yaw), Pitch: f(value.Pitch), Roll: f(value.Roll));

        private ActivationToggled? ActivationToggled(Contrib.Data.DRS drs, Contrib.Data.Shared sharedData)
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
        
        private WaitTimeToggled? WaitTimeToggled(Contrib.Data.PushToPass ptp, Contrib.Data.Shared sharedData)
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

        private Tyre[][] Tyres(Contrib.Data.Shared sharedData)
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

        private Tyre Tyre(Contrib.Data.Shared sharedData, ITyreExtractor extract)
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

        private IBoundedValue<UInt32>? BlueFlagWarnings(Int32 blackAndWhite)
        {
            UInt32 blueWarnings = blackAndWhite switch {
                1 => 1,
                2 => 2,
                _ => 0
            };
            return new BoundedValue<UInt32>(blueWarnings, 2);
        }

        private UInt32 PositiveOrZero(Int32 value) => value > 0 ? (UInt32)value : 0;

        private String? FromNullTerminatedByteArray(byte[] nullTerminated)
        {
            var nullIndex = Array.IndexOf<byte>(nullTerminated, 0);
            if (nullIndex <= 0)
                return null;
            return Encoding.UTF8.GetString(nullTerminated, 0, nullIndex);
        }

        private Vector3<O> Vector3<I, O>(Contrib.Data.Vector3<I> value, Func<I, O> f) =>
            new Vector3<O>(X: f(value.X), Y: f(value.Y), Z: f(value.Z));

        private O[] ValuesPerSector<I, O>(Contrib.Data.Sectors<I> value, Func<I, O> f) =>
            new O[] { f(value.Sector1), f(value.Sector2), f(value.Sector3) };

        private O[] ValuesPerSector<I, O>(Contrib.Data.SectorStarts<I> value, Func<I, O> f) =>
            new O[] { f(value.Sector1), f(value.Sector2), f(value.Sector3) };

        private UInt32 SafeUInt32(Int32 i, UInt32 defaultValue = 0)
        {
            if (i < 0)
                return defaultValue;
            else
                return (UInt32)i;
        }

        private Boolean? NullableBoolean(Int32 i)
        {
            if (i < 0)
                return null;
            if (i > 0)
                return true;
            return false;
        }
    }

    /// <summary>
    /// Keeps track of the previous aid value for when its activated value hides it.
    /// </summary>
    public class StatefulAid<T> where T : Aid
    {
        private T? current = null;
        private Func<UInt32, T> constructor;

        public StatefulAid(Func<UInt32, T> constructor)
        {
            this.constructor = constructor;
        }

        public T? Update(Int32 newLevel)
        {
            switch (newLevel)
            {
                case < 0:
                    current = null;
                    break;
                case 5:
                    if (current != null)
                        current = current with { Active = true };
                    break;
                default:
                    current = constructor((UInt32)newLevel);
                    break;
            }
            return current;
        }
    }

    public static class StatefulAid
    {
        public static StatefulAid<Aid> Generic() =>
            new StatefulAid<Aid>(level => new Aid(level, false));

        public static StatefulAid<TractionControl> Tc() =>
            new StatefulAid<TractionControl>(level => new TractionControl(level, false, null));
    }
}
