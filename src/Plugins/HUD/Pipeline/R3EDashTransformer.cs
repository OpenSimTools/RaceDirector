using RaceDirector.Pipeline.Telemetry.V0;
using System.Text.Json;
using System.IO;
using System;
using RaceDirector.Plugin.HUD.Utils;
using static RaceDirector.Pipeline.Telemetry.V0.RaceDuration;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using System.Text;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public static class R3EDashTransformer
    {
        public static readonly uint MajorVersion = 2;
        public static readonly uint MinorVersion = 11;

        public static readonly int DecimalDigits = 3;

        private static readonly JsonWriterOptions JsonWriterOptions = new JsonWriterOptions();

        public static byte[] ToR3EDash(IGameTelemetry gt)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, JsonWriterOptions))
                {
                    WriteR3EDash(writer, gt);
                }
                return stream.ToArray();
            }
        }

        private static void WriteR3EDash(Utf8JsonWriter w, IGameTelemetry gt)
        {
            w.WriteObject(_ =>
            {
                w.WriteNumber("VersionMajor", MajorVersion);
                w.WriteNumber("VersionMinor", MinorVersion);

                // AllDriversOffset
                // DriverDataSize

                // GamePaused

                w.WriteNumber("GameInMenus", gt.Session is null ? -1 : MatchAsInt32(gt.GameState, GameState.Menu));
                w.WriteNumber("GameInReplay", gt.Session is null ? -1 : MatchAsInt32(gt.GameState, GameState.Replay));
                w.WriteNumber("GameUsingVr", gt.Session is null ? -1 : ToInt32(gt.UsingVR));

                w.WriteObject("Player", _ =>
                {
                    // Player.GameSimulationTicks
                    // Player.GameSimulationTime

                    w.WriteCoordinates("Position", gt.Player?.CgLocation, d => d.M);

                    // Player.Velocity.X
                    // Player.Velocity.Y
                    // Player.Velocity.Z
                    // Player.LocalVelocity.X
                    // Player.LocalVelocity.Y
                    // Player.LocalVelocity.Z
                    // Player.Acceleration.X
                    // Player.Acceleration.Y
                    // Player.Acceleration.Z

                    w.WriteCoordinates("LocalAcceleration", gt.Player?.LocalAcceleration, a => a.MPS2);

                    // Player.Orientation.X
                    // Player.Orientation.Y
                    // Player.Orientation.Z
                    // Player.Rotation.X
                    // Player.Rotation.Y
                    // Player.Rotation.Z
                    // Player.AngularAcceleration.X
                    // Player.AngularAcceleration.Y
                    // Player.AngularAcceleration.Z
                    // Player.AngularVelocity.X
                    // Player.AngularVelocity.Y
                    // Player.AngularVelocity.Z
                    // Player.LocalAngularVelocity.X
                    // Player.LocalAngularVelocity.Y
                    // Player.LocalAngularVelocity.Z

                    w.WriteCoordinates("LocalGforce", gt.Player?.LocalAcceleration, a => a.ApproxG);

                    // Player.SteeringForce
                    // Player.SteeringForcePercentage
                    // Player.EngineTorque
                    // Player.CurrentDownforce
                    // Player.Voltage
                    // Player.ErsLevel
                    // Player.PowerMguH
                    // Player.PowerMguK
                    // Player.TorqueMguK
                    // Player.SuspensionDeflection.FrontLeft
                    // Player.SuspensionDeflection.FrontRight
                    // Player.SuspensionDeflection.RearLeft
                    // Player.SuspensionDeflection.RearRight
                    // Player.SuspensionVelocity.FrontLeft
                    // Player.SuspensionVelocity.FrontRight
                    // Player.SuspensionVelocity.RearLeft
                    // Player.SuspensionVelocity.RearRight
                    // Player.Camber.FrontLeft
                    // Player.Camber.FrontRight
                    // Player.Camber.RearLeft
                    // Player.Camber.RearRight
                    // Player.RideHeight.FrontLeft
                    // Player.RideHeight.FrontRight
                    // Player.RideHeight.RearLeft
                    // Player.RideHeight.RearRight
                    // Player.FrontWingHeight
                    // Player.FrontRollAngle
                    // Player.RearRollAngle
                    // Player.thirdSpringSuspensionDeflectionFront
                    // Player.thirdSpringSuspensionVelocityFront
                    // Player.thirdSpringSuspensionDeflectionRear
                    // Player.thirdSpringSuspensionVelocityRear
                });

                // TrackName
                // LayoutName
                // TrackId
                // LayoutId

                w.WriteRoundedNumber("LayoutLength", (gt.Event?.Track.Length?.M) ?? -1.0);

                w.WriteSectors("SectorStartFactors", gt.Event?.Track.SectorsEnd, st => st.Fraction);

                // RaceSessionLaps.Race1
                // RaceSessionLaps.Race2
                // RaceSessionLaps.Race3
                // RaceSessionMinutes.Race1
                // RaceSessionMinutes.Race2
                // RaceSessionMinutes.Race3
                // EventIndex

                w.WriteNumber("SessionType", gt.Session?.Type switch
                {
                    SessionType.Practice => 0,
                    SessionType.Qualify => 1,
                    SessionType.Race => 2,
                    SessionType.Warmup => 3,
                    _ => -1
                });

                // SessionIteration
                // SessionLengthFormat

                w.WriteNumber("SessionLengthFormat", gt.Session?.Length switch
                {
                    RaceDuration.TimeDuration => 0,
                    RaceDuration.LapsDuration => 1,
                    RaceDuration.TimePlusLapsDuration => 2,
                    _ => -1
                });
                w.WriteRoundedNumber("SessionPitSpeedLimit", gt.Session?.PitSpeedLimit.MPS ?? -1.0);
                w.WriteNumber("SessionPhase", gt.Session?.Phase switch
                {
                    SessionPhase.Garage => 1,
                    SessionPhase.Gridwalk => 2,
                    SessionPhase.Formation => 3,
                    SessionPhase.Countdown => 4,
                    SessionPhase.Started => 5,
                    SessionPhase.Over => 6,
                    _ => -1
                });
                w.WriteNumber("StartLights", ToInt32(gt.Session?.StartLights));

                // TireWearActive
                w.WriteNumber("FuelUseActive", Convert.ToInt32(gt.Event?.FuelRate ?? -1));

                w.WriteNumber("NumberOfLaps", gt.Session?.Length switch
                {
                    LapsDuration length => length.Laps,
                    _ => -1
                });
                w.WriteRoundedNumber("SessionTimeDuration", gt.Session?.Length switch
                {
                    TimeDuration length => length.Time.TotalSeconds,
                    TimePlusLapsDuration length => length.Time.TotalSeconds,
                    _ => -1.0
                });
                w.WriteRoundedNumber("SessionTimeRemaining",
                    gt.Session?.WaitTime?.TotalSeconds ??
                    gt.Session?.TimeRemaining?.TotalSeconds ??
                    -1.0
                );

                // MaxIncidentPoints

                w.WriteNumber("PitWindowStatus", PitWindowStatusAsInt32(gt));

                var pitWindow = PitWindowBoundaries(gt);
                w.WriteNumber("PitWindowStart", pitWindow.Start);
                w.WriteNumber("PitWindowEnd", pitWindow.Finish);

                w.WriteNumber("InPitLane", InPitLaneAsInt32(gt.FocusedVehicle));

                // PitMenuSelection
                // PitMenuState.Preset
                // PitMenuState.Penalty
                // PitMenuState.Driverchange
                // PitMenuState.Fuel
                // PitMenuState.FrontTires
                // PitMenuState.RearTires
                // PitMenuState.FrontWing
                // PitMenuState.RearWing
                // PitMenuState.Suspension
                // PitMenuState.ButtonTop
                // PitMenuState.ButtonBottom

                w.WriteNumber("PitState", PitStateAsInt32(gt));
                w.WriteRoundedNumber("PitTotalDuration", gt.FocusedVehicle?.Pit.PitLaneTime?.TotalSeconds ?? -1.0);
                w.WriteRoundedNumber("PitElapsedTime", gt.FocusedVehicle?.Pit.PitStallTime?.TotalSeconds ?? -1.0);
                w.WriteNumber("PitAction", PitActionAsInt32(gt.Player));

                // NumPitstopsPerformed
                // PitMinDurationTotal
                // PitMinDurationLeft

                w.WriteObject("Flags", _ =>
                {
                    var raceStarted = gt.Session?.Phase switch
                    {
                        SessionPhase.Started => true,
                        SessionPhase.FullCourseYellow => true,
                        SessionPhase.Stopped => true,
                        SessionPhase.Over => true,
                        _ => false
                    };
                    var focusedVechicleRacing = gt.FocusedVehicle?.RacingStatus == IRacingStatus.Racing; // TODO test
                    var raceOngoing = gt.Session?.Phase switch
                    {
                        SessionPhase.Started => true,
                        SessionPhase.FullCourseYellow => true,
                        SessionPhase.Stopped => true,
                        SessionPhase.Over => focusedVechicleRacing, // Race is over when the leader crosses the line
                        _ => false
                    };

                    w.WriteNumber("Yellow", ToInt32(gt.FocusedVehicle?.Flags, f => ConditionalFlagAsInt32(f.Yellow, raceOngoing)));

                    // Flags.YellowCausedIt

                    w.WriteNumber("YellowOvertake", ToInt32(gt.Player?.OvertakeAllowed));
                    w.WriteNumber("YellowPositionsGained", Conditional(ToInt32(gt.Player?.Warnings.GiveBackPositions), raceStarted));

                    // Flags.SectorYellow.Sector1
                    // Flags.SectorYellow.Sector2
                    // Flags.SectorYellow.Sector3
                    // Flags.ClosestYellowDistanceIntoTrack

                    w.WriteNumber("Blue", ToInt32(gt.FocusedVehicle?.Flags, f => ConditionalFlagAsInt32(f.Blue, raceOngoing)));
                    w.WriteNumber("Black", ToInt32(gt.FocusedVehicle?.Flags, f => FlagAsInt32(f.Black)));
                    w.WriteNumber("Green", ToInt32(gt.FocusedVehicle?.Flags, f => ConditionalFlagAsInt32(f.Green, raceOngoing)));
                    w.WriteNumber("Checkered", ToInt32(gt.FocusedVehicle?.Flags, f => FlagAsInt32(f.Chequered)));
                    w.WriteNumber("White", ToInt32(gt.FocusedVehicle?.Flags, f => ConditionalFlagAsInt32(f.White, raceOngoing)));
                    w.WriteNumber("BlackAndWhite", ToInt32(gt.FocusedVehicle?.Flags,
                        f => BlackWhiteFlagAsInt32(f.BlackWhite, gt.Player?.Warnings.BlueFlagWarnings?.Value, raceOngoing)
                    ));
                });

                // Position

                w.WriteNumber("PositionClass", ToInt32(gt.FocusedVehicle?.PositionClass));

                // FinishStatus
                // CutTrackWarnings

                w.WriteObject("Penalties", _ =>
                {
                    var penalties = gt.FocusedVehicle?.Penalties;
                    var initValue = penalties is null ? -1 : 0;
                    var driveThrough = initValue;
                    var stopAndGo = initValue;
                    var pitStop = initValue;
                    var timeDeduction = initValue;
                    var slowDown = initValue;
                    foreach (var p in penalties ?? Array.Empty<IPenalty>())
                    {
                        switch (p.Type)
                        {
                            case PenaltyType.DriveThrough:
                                driveThrough = 1;
                                break;
                            case PenaltyType.StopAndGo10:
                            case PenaltyType.StopAndGo20:
                            case PenaltyType.StopAndGo30:
                                stopAndGo = 1;
                                break;
                            case PenaltyType.PitStop:
                                pitStop = 1;
                                break;
                            case PenaltyType.TimeDeduction:
                                timeDeduction = 1;
                                break;
                            case PenaltyType.SlowDown:
                                slowDown = 1;
                                break;
                        }
                    }

                    w.WriteNumber("DriveThrough", driveThrough);
                    w.WriteNumber("StopAndGo", stopAndGo);
                    w.WriteNumber("PitStop", pitStop);
                    w.WriteNumber("TimeDeduction", timeDeduction);
                    w.WriteNumber("SlowDown", slowDown);
                });

                // NumPenalties

                w.WriteNumber("CompletedLaps", ToInt32(gt.FocusedVehicle?.CompletedLaps));
                w.WriteNumber("CurrentLapValid", gt.FocusedVehicle?.LapValid switch {
                    LapValidState.Invalid => 0,
                    LapValidState.Valid => 1,
                    _ => -1
                });

                // TrackSector

                w.WriteRoundedNumber("LapDistance", gt.FocusedVehicle?.CurrentLapDistance.Value.M ?? -1.0);
                w.WriteRoundedNumber("LapDistanceFraction", gt.FocusedVehicle?.CurrentLapDistance.Fraction ?? -1.0);

                // LapTimeBestLeader
                // LapTimeBestLeaderClass
                // SectorTimesSessionBestLap.Sector1
                // SectorTimesSessionBestLap.Sector2
                // SectorTimesSessionBestLap.Sector3

                w.WriteRoundedNumber("LapTimeBestSelf", gt.FocusedVehicle?.BestLapTime?.Overall.TotalSeconds ?? -1.0);
                w.WriteSectors("SectorTimesBestSelf", gt.FocusedVehicle?.BestLapTime?.Sectors.Cumulative, st => st.TotalSeconds);

                // LapTimePreviousSelf
                // SectorTimesPreviousSelf.Sector1
                // SectorTimesPreviousSelf.Sector2
                // SectorTimesPreviousSelf.Sector3

                w.WriteRoundedNumber("LapTimeCurrentSelf", gt.FocusedVehicle?.CurrentLapTime?.Overall.TotalSeconds ?? -1.0);
                w.WriteSectors("SectorTimesCurrentSelf", gt.FocusedVehicle?.CurrentLapTime?.Sectors.Cumulative, st => st.TotalSeconds);

                // LapTimeDeltaLeader
                // LapTimeDeltaLeaderClass
                // TimeDeltaFront => Player or -1.0
                // TimeDeltaBehind => Player or -1.0

                w.WriteRoundedNumber("TimeDeltaBestSelf", gt.Player?.PersonalBestDelta?.TotalSeconds ?? -1000.0);

                w.WriteSectors("BestIndividualSectorTimeSelf", gt.Player?.PersonalBestSectors?.Individual, st => st.TotalSeconds);

                // BestIndividualSectorTimeLeader.Sector1
                // BestIndividualSectorTimeLeader.Sector2
                // BestIndividualSectorTimeLeader.Sector3

                w.WriteSectors("BestIndividualSectorTimeLeaderClass", gt.Player?.ClassBestSectors?.Individual, st => st.TotalSeconds);

                // IncidentPoints

                w.WriteObject("VehicleInfo", _ =>
                {
                    // VehicleInfo.Name
                    // VehicleInfo.CarNumber
                    // VehicleInfo.ClassId
                    // VehicleInfo.ModelId
                    // VehicleInfo.TeamId
                    // VehicleInfo.LiveryId
                    // VehicleInfo.ManufacturerId
                    // VehicleInfo.UserId

                    w.WriteNumber("SlotId", ToInt32(gt.FocusedVehicle?.Id));
                    w.WriteNumber("ClassPerformanceIndex", gt.FocusedVehicle?.ClassPerformanceIndex ?? -1);
                    w.WriteNumber("EngineType", gt.FocusedVehicle?.EngineType switch {
                        EngineType.Combustion => 0,
                        EngineType.Electric => 1,
                        EngineType.Hybrid => 2,
                        _ => 3 // !!!
                    });
                });

                // NOTE it is the current vehicle's driver name rather than player name!
                w.WriteBase64String("PlayerName", NullTerminated(gt.FocusedVehicle?.CurrentDriver.Name));

                w.WriteNumber("ControlType", gt.FocusedVehicle?.ControlType switch
                {
                    ControlType.LocalPlayer => 0,
                    ControlType.AI => 1,
                    ControlType.RemotePlayer => 2,
                    ControlType.Replay => 3,
                    _ => -1
                });

                w.WriteRoundedNumber("CarSpeed", gt.FocusedVehicle?.Speed.MPS ?? -1.0);
                
                w.WriteRoundedNumber("EngineRps", gt.Player?.Engine.Speed.RadPS ?? -1.0);
                w.WriteRoundedNumber("MaxEngineRps", gt.Player?.Engine.MaxSpeed.RadPS ?? -1.0);
                w.WriteRoundedNumber("UpshiftRps", gt.Player?.Engine.UpshiftSpeed.RadPS ?? -1.0);

                // Gear
                // NumGears

                w.WriteCoordinates("CarCgLocation", gt.FocusedVehicle?.Location, d => d.M);

                w.WriteOrientationPYR("CarOrientation", gt.FocusedVehicle?.Orientation, a => a.Rad);

                // LocalAcceleration.X
                // LocalAcceleration.Y
                // LocalAcceleration.Z
                // TotalMass

                w.WriteRoundedNumber("FuelLeft", gt.Player?.Fuel.Left ?? -1.0);
                w.WriteRoundedNumber("FuelCapacity", gt.Player?.Fuel.Max ?? -1.0);
                w.WriteRoundedNumber("FuelPerLap", gt.Player?.Fuel.PerLap ?? -1.0);

                // EngineWaterTemp
                // EngineOilTemp
                // FuelPressure
                // EngineOilPressure
                // TurboPressure

                w.WriteRoundedNumber("Throttle", gt.FocusedVehicle?.Inputs?.Throttle ?? -1.0);
                w.WriteRoundedNumber("ThrottleRaw", gt.Player?.RawInputs.Throttle ?? -1.0);
                w.WriteRoundedNumber("Brake", gt.FocusedVehicle?.Inputs?.Brake ?? -1.0);
                w.WriteRoundedNumber("BrakeRaw", gt.Player?.RawInputs.Brake ?? -1.0);
                w.WriteRoundedNumber("Clutch", gt.FocusedVehicle?.Inputs?.Clutch ?? -1.0);
                w.WriteRoundedNumber("ClutchRaw", gt.Player?.RawInputs.Clutch ?? -1.0);
                w.WriteRoundedNumber("SteerInputRaw", gt.Player?.RawInputs.Steering ?? 0.0);

                // SteerLockDegrees

                w.WriteNumber("SteerWheelRangeDegrees", ToUInt32(gt.Player?.RawInputs.SteerWheelRange.Deg));

                w.WriteObject("AidSettings", _ =>
                {
                    w.WriteNumber("Abs", ToInt32(gt.Player?.DrivingAids.Abs));
                    w.WriteNumber("Tc", ToInt32(gt.Player?.DrivingAids.Tc));
                    w.WriteNumber("Esp", ToInt32(gt.Player?.DrivingAids.Esp));
                    w.WriteNumber("Countersteer", ToInt32(gt.Player?.DrivingAids.Countersteer));
                    w.WriteNumber("Cornering", ToInt32(gt.Player?.DrivingAids.Cornering));
                });

                w.WriteObject("Drs", _ =>
                {
                    if (gt.Player is null)
                    {
                        w.WriteNumber("Equipped", -1);
                        w.WriteNumber("Available", -1);
                        w.WriteNumber("NumActivationsLeft", -1);
                        w.WriteNumber("Engaged", -1);
                    }
                    else if (gt.Player.Drs is null)
                    {
                        w.WriteNumber("Equipped", 0);
                        w.WriteNumber("Available", 0);
                        w.WriteNumber("NumActivationsLeft", 0);
                        w.WriteNumber("Engaged", 0);
                    }
                    else
                    {
                        w.WriteNumber("Equipped", 1);
                        w.WriteNumber("Available", gt.Player.Drs.Available ? 1 : 0);
                        w.WriteNumber("NumActivationsLeft", ToInt32(gt.Player.Drs.ActivationsLeft?.Value));
                        w.WriteNumber("Engaged", gt.Player.Drs.Engaged ? 1 : 0);
                    }
                });

                // PitLimiter

                w.WriteObject("PushToPass", _ =>
                {
                    w.WriteNumber("Available", ToInt32(gt.Player?.PushToPass?.Available));
                    w.WriteNumber("Engaged", ToInt32(gt.Player?.PushToPass?.Engaged));
                    w.WriteNumber("AmountLeft", ToInt32(gt.Player?.PushToPass?.ActivationsLeft?.Value));
                    w.WriteRoundedNumber("EngagedTimeLeft", gt.Player?.PushToPass?.EngagedTimeLeft.TotalSeconds ?? -1.0); // not sure
                    w.WriteRoundedNumber("WaitTimeLeft", gt.Player?.PushToPass?.WaitTimeLeft.TotalSeconds ?? -1.0); // not sure
                });

                // BrakeBias

                w.WriteNumber("DrsNumActivationsTotal", ToInt32(gt.Player?.Drs?.ActivationsLeft?.Total));
                w.WriteNumber("PtPNumActivationsTotal", ToInt32(gt.Player?.PushToPass?.ActivationsLeft?.Total)); // ********************** is this per lap?!?!?!?!?!?

                // TireType
                // TireRps.FrontLeft
                // TireRps.FrontRight
                // TireRps.RearLeft
                // TireRps.RearRight
                // TireSpeed.FrontLeft
                // TireSpeed.FrontRight
                // TireSpeed.RearLeft
                // TireSpeed.RearRight

                w.WriteObject("TireGrip", _ =>
                {
                    ForEachTyre(gt.Player?.Tyres, (tyreName, tyre) =>
                    {
                        w.WriteRoundedNumber(tyreName, tyre?.Grip ?? -1.0);
                    });
                });

                w.WriteObject("TireWear", _ =>
                {
                    ForEachTyre(gt.Player?.Tyres, (tyreName, tyre) =>
                    {
                        w.WriteRoundedNumber(tyreName, tyre?.Wear ?? -1.0);
                    });
                });

                // TireFlatspot.FrontLeft
                // TireFlatspot.FrontRight
                // TireFlatspot.RearLeft
                // TireFlatspot.RearRight
                // TirePressure.FrontLeft
                // TirePressure.FrontRight
                // TirePressure.RearLeft
                // TirePressure.RearRight

                w.WriteObject("TireDirt", _ =>
                {
                    ForEachTyre(gt.Player?.Tyres, (tyreName, tyre) =>
                    {
                        w.WriteRoundedNumber(tyreName, tyre?.Dirt ?? -1.0);
                    });
                });

                w.WriteObject("TireTemp", _ =>
                {
                    ForEachTyre(gt.Player?.Tyres, (tyreName, tyre) =>
                    {
                        w.WriteObject(tyreName, _ =>
                        {
                            var temperatures = tyre?.Temperatures;
                            w.WriteObject("CurrentTemp", _ =>
                            {
                                var currentTemperatures = temperatures?.CurrentTemperatures;
                                w.WriteRoundedNumber("Left", currentTemperatures?.GetValueOrNull(0, 0)?.C ?? -1.0);
                                w.WriteRoundedNumber("Center", currentTemperatures?.GetValueOrNull(0, 1)?.C ?? -1.0);
                                w.WriteRoundedNumber("Right", currentTemperatures?.GetValueOrNull(0, 2)?.C ?? -1.0);
                            });
                            w.WriteRoundedNumber("OptimalTemp", temperatures?.OptimalTemperature.C ?? -1.0);
                            w.WriteRoundedNumber("ColdTemp", temperatures?.ColdTemperature.C ?? -1.0);
                            w.WriteRoundedNumber("HotTemp", temperatures?.HotTemperature.C ?? -1.0);
                        });
                    });
                });

                // TireTypeFront
                // TireTypeRear
                // TireSubtypeFront
                // TireSubtypeRear

                w.WriteObject("BrakeTemp", _ =>
                {
                    ForEachTyre(gt.Player?.Tyres, (tyreName, tyre) =>
                    {
                        w.WriteObject(tyreName, _ =>
                        {
                            w.WriteRoundedNumber("CurrentTemp", tyre?.BrakeTemperatures.CurrentTemperature.C ?? -1.0);
                            w.WriteRoundedNumber("OptimalTemp", tyre?.BrakeTemperatures.OptimalTemperature.C ?? - 1.0);
                            w.WriteRoundedNumber("ColdTemp", tyre?.BrakeTemperatures.ColdTemperature.C ?? - 1.0);
                            w.WriteRoundedNumber("HotTemp", tyre?.BrakeTemperatures.HotTemperature.C ?? - 1.0);
                        });
                    });
                });

                // BrakePressure.FrontLeft
                // BrakePressure.FrontRight
                // BrakePressure.RearLeft
                // BrakePressure.RearRight
                // TractionControlSetting
                // EngineMapSetting
                // EngineBrakeSetting
                // TireLoad.FrontLeft
                // TireLoad.FrontRight
                // TireLoad.RearLeft
                // TireLoad.RearRight

                w.WriteObject("CarDamage", _ =>
                {
                    w.WriteRoundedNumber("Engine", gt.Player?.VehicleDamage.EnginePercent ?? -1.0);
                    w.WriteRoundedNumber("Transmission", gt.Player?.VehicleDamage.TransmissionPercent ?? -1.0);
                    w.WriteRoundedNumber("Aerodynamics", gt.Player?.VehicleDamage.AerodynamicsPercent ?? -1.0);
                    w.WriteRoundedNumber("Suspension", gt.Player?.VehicleDamage.SuspensionPercent ?? -1.0);
                });

                // NumCars

                // Note: Sometimes there's a lot of "null" vehicles after NumCars, but we won't render them.
                w.WriteArray("DriverData", _ =>
                {
                    foreach (var vehicle in gt.Vehicles)
                    {
                        w.WriteObject(_ =>
                        {
                            w.WriteObject("DriverInfo", _ =>
                            {
                                w.WriteBase64String("Name", NullTerminated(vehicle.CurrentDriver.Name));

                                // DriverData[].DriverInfo.CarNumber
                                // DriverData[].DriverInfo.ClassId
                                // DriverData[].DriverInfo.ModelId
                                // DriverData[].DriverInfo.TeamId
                                // DriverData[].DriverInfo.LiveryId
                                // DriverData[].DriverInfo.ManufacturerId
                                // DriverData[].DriverInfo.UserId

                                w.WriteNumber("SlotId", vehicle.Id);
                                w.WriteNumber("ClassPerformanceIndex", vehicle.ClassPerformanceIndex);

                                // DriverData[].DriverInfo.EngineType
                            });

                            // DriverData[].FinishStatus
                            // DriverData[].Place

                            w.WriteNumber("PlaceClass", vehicle.PositionClass);
                            w.WriteRoundedNumber("LapDistance", vehicle.CurrentLapDistance.Value.M);
                            w.WriteCoordinates("Position", vehicle.Location, d => d.M);

                            // DriverData[].TrackSector

                            w.WriteNumber("CompletedLaps", vehicle.CompletedLaps);

                            // DriverData[].CurrentLapValid
                            // DriverData[].LapTimeCurrentSelf
                            // DriverData[].SectorTimeCurrentSelf.Sector1
                            // DriverData[].SectorTimeCurrentSelf.Sector2
                            // DriverData[].SectorTimeCurrentSelf.Sector3
                            // DriverData[].SectorTimePreviousSelf.Sector1
                            // DriverData[].SectorTimePreviousSelf.Sector2
                            // DriverData[].SectorTimePreviousSelf.Sector3

                            w.WriteSectors("SectorTimeBestSelf", vehicle.BestLapTime?.Sectors.Cumulative, st => st.TotalSeconds);
                            w.WriteRoundedNumber("TimeDeltaFront", vehicle.GapAhead?.TotalSeconds ?? -1.0);
                            w.WriteRoundedNumber("TimeDeltaBehind", vehicle.GapBehind?.TotalSeconds ?? -1.0);

                            // DriverData[].PitStopStatus
                            // DriverData[].InPitlane
                            // DriverData[].NumPitstops
                            // DriverData[].Penalties.DriveThrough
                            // DriverData[].Penalties.StopAndGo
                            // DriverData[].Penalties.PitStop
                            // DriverData[].Penalties.TimeDeduction
                            // DriverData[].Penalties.SlowDown
                            // DriverData[].CarSpeed
                            // DriverData[].TireTypeFront
                            // DriverData[].TireTypeRear
                            // DriverData[].TireSubtypeFront
                            // DriverData[].TireSubtypeRear
                            // DriverData[].BasePenaltyWeight
                            // DriverData[].AidPenaltyWeight
                            // DriverData[].DrsState
                            // DriverData[].PtpState
                            // DriverData[].PenaltyType
                            // DriverData[].PenaltyReason
                        });
                    }
                });
            });
        }

        private static Interval<int> PitWindowBoundaries(IGameTelemetry gt) =>
            (gt.Session?.Requirements.PitWindow) switch
            {
                Interval<IPitWindowBoundary>(LapsDuration start, LapsDuration finish)
                        when gt.FocusedVehicle?.CompletedLaps.CompareTo(finish.Laps) < 0 =>
                    new Interval<int>(Convert.ToInt32(start.Laps), Convert.ToInt32(finish.Laps)),
                Interval<IPitWindowBoundary>(TimeDuration start, TimeDuration finish)
                        when gt.Session.ElapsedTime < finish.Time =>
                    new Interval<int>(Convert.ToInt32(start.Time.TotalMinutes), Convert.ToInt32(finish.Time.TotalMinutes)),
                _ =>
                    new Interval<int>(-1, -1)
            };

        public static void WriteRoundedNumber(this Utf8JsonWriter writer, String propertyName, double value)
        {
            writer.WriteNumber(propertyName, value, DecimalDigits);
        }

        private static void WriteSectors<T>(this Utf8JsonWriter writer, String propertyName, T[]? v, Func<T, double> f)
        {
            writer.WriteObject(propertyName, _ =>
            {
                for (int i = 0; i < 3; i++)
                {
                    writer.WriteRoundedNumber("Sector" + (i + 1), v?.Length > i ? f(v[i]) : -1.0);
                }
            });
        }

        private static void WriteCoordinates<T>(this Utf8JsonWriter writer, String propertyName, Vector3<T>? v, Func<T, double> f)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteRoundedNumber("X", v is not null ? f(v.X) : 0.0);
                writer.WriteRoundedNumber("Y", v is not null ? f(v.Y) : 0.0);
                writer.WriteRoundedNumber("Z", v is not null ? f(v.Z) : 0.0);
            });
        }

        private static void WriteOrientationPYR(this Utf8JsonWriter writer, String propertyName, Orientation? v, Func<IAngle, double> f)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteRoundedNumber("Pitch", v is not null ? f(v.Pitch) : 0.0);
                writer.WriteRoundedNumber("Yaw", v is not null ? f(v.Yaw) : 0.0);
                writer.WriteRoundedNumber("Roll", v is not null ? f(v.Roll) : 0.0);
            });
        }

        private static void WriteOrientationXYZ(this Utf8JsonWriter writer, String propertyName, Orientation? v, Func<IAngle, double> f)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteRoundedNumber("X", v is not null ? f(v.Pitch) : 0.0);
                writer.WriteRoundedNumber("Y", v is not null ? f(v.Yaw) : 0.0);
                writer.WriteRoundedNumber("Z", v is not null ? f(v.Roll) : 0.0);
            });
        }

        private static void ForEachTyre(ITyre[][]? tyres, Action<String, ITyre?> action)
        {
            action("FrontLeft", tyres?.GetValueOrNull(0, 0));
            action("FrontRight", tyres?.GetValueOrNull(0, 1));
            action("RearLeft", tyres?.GetValueOrNull(1, 0));
            action("RearRight", tyres?.GetValueOrNull(1, 1));
        }

        private static T? GetValueOrNull<T>(this T[][] array, int i, int j) =>
            (i < array.Length && j < array[i].Length) ? array[i][j] : default(T);


        private static byte[] NullTerminated(String? value) => Encoding.UTF8.GetBytes($"{value ?? ""}\0");

        private static uint ToUInt32(double? value)
        {
            try
            {
                return Convert.ToUInt32(value);
            } catch
            {
                return 0;
            }
        }

        private static int ToInt32(uint? value)
        {
            if (value is null || value > int.MaxValue)
                return -1;
            return Convert.ToInt32(value);
        }

        private static int ToInt32(bool? value)
        {
            return MatchAsInt32(value, true);
        }

        private static int MatchAsInt32<T>(T? value, T constant)
        {
            return ToInt32(value, v => ToInt32(v.Equals(constant)));
        }

        private static int ToInt32(IStartLights? startLights)
        {
            return ToInt32(startLights, sl =>
            {
                if (sl.Colour == LightColour.Green)
                    return 6;
                else
                    return Convert.ToInt32(5 * sl.Lit.Value / sl.Lit.Total);
            });
        }

        private static int ToInt32<T>(T? value, Func<T, int> f)
        {
            if (value is null)
                return -1;
            return f(value);
        }

        private static int ToInt32(bool value)
        {
            return value ? 1 : 0;
        }

        private static int ToInt32(IAid? aid)
        {
            if (aid?.Active == true)
                return 5;
            return ToInt32(aid?.Level);
        }

        private static int PitWindowStatusAsInt32(IGameTelemetry gt)
        {
            var s = gt.Session;
            if (s is null)
                return -1; // Unavailable
            if (!s.PitLaneOpen)
                return 0; // Disabled

            var fv = gt.FocusedVehicle;
            if (fv is null)
                return -1; // Unavailable

            if (fv.Pit.PitLanePhase == PitLanePhase.Stopped)
                return 3; // Stopped

            var pitWindow = s.Requirements.PitWindow;
            if (pitWindow is null)
                return 2; // Open

            if (fv.Pit.MandatoryStopsDone >= s.Requirements.MandatoryPitStops)
                return 4; // Completed

            // This should stay open even when in the pits when the time expires.
            // Not worth implementing it though, as it's both difficult and pointless.
            var raceInstant = new RaceInstant(s.ElapsedTime, fv.CompletedLaps);

            if (raceInstant.IsWithin(pitWindow))
                return 2;

            return 1; // Closed
        }

        private static int InPitLaneAsInt32(IVehicle? vehicle)
        {
            if (vehicle is null)
                return -1;
            if (vehicle.Pit.PitLanePhase is null)
                return 0;
            return 1;
        }

        private static int PitStateAsInt32(IGameTelemetry gt)
        {
            if (gt.FocusedVehicle is null)
                return -1;
            switch (gt.FocusedVehicle?.Pit.PitLanePhase)
            {
                case PitLanePhase.Entered:
                    return 2;
                case PitLanePhase.Stopped:
                    return 3;
                case PitLanePhase.Exiting:
                    return 4;
            }
            if (gt.Player?.PitStopStatus.HasFlag(PlayerPitStop.Requested) ?? false)
                return 1;
            return 0;
        }

        private static int PitActionAsInt32(IPlayer? player)
        {
            if (player is null)
                return -1;
            int pitAction = 0;
            foreach (var (playerPitstopFlag, pitActionFlag) in pitActionMapping)
                if (player.PitStopStatus.HasFlag(playerPitstopFlag)) pitAction += pitActionFlag;
            return pitAction;
        }

        private static readonly (PlayerPitStop, int)[] pitActionMapping = {
                (PlayerPitStop.Preparing,        1 << 0),
                (PlayerPitStop.ServingPenalty,   1 << 1),
                (PlayerPitStop.DriverChange,     1 << 2),
                (PlayerPitStop.Refuelling,       1 << 3),
                (PlayerPitStop.ChangeFrontTyres, 1 << 4),
                (PlayerPitStop.ChangeRearTyres,  1 << 5),
                (PlayerPitStop.RepairBody,       1 << 6),
                (PlayerPitStop.RepairFrontWing,  1 << 7),
                (PlayerPitStop.RepairRearWing,   1 << 8),
                (PlayerPitStop.RepairSuspension, 1 << 9)
            };


        private static int BlackWhiteFlagAsInt32(IVehicleFlags.IBlackWhite? blackWhiteFlag, uint? blueFlagWarnings, bool raceStarted)
        {
            if (!raceStarted)
                return -1;
            return (blackWhiteFlag?.Reason, blueFlagWarnings) switch
            {
                (IVehicleFlags.BlackWhiteReason.IgnoredBlueFlags, 1) => 1,
                (IVehicleFlags.BlackWhiteReason.IgnoredBlueFlags, 2) => 2,
                (IVehicleFlags.BlackWhiteReason.WrongWay, _) => 3,
                (IVehicleFlags.BlackWhiteReason.Cutting, _) => 4,
                _ => 0
            };
        }

        private static int FlagAsInt32(IVehicleFlags.IFlag? flag) =>
            ToInt32(flag is not null);

        private static int ConditionalFlagAsInt32(IVehicleFlags.IFlag? flag, bool raceStarted) =>
            Conditional(ToInt32(flag is not null), raceStarted);

        private static int Conditional(int value, bool raceStarted) =>
            raceStarted ? value : -1;
    }
}
