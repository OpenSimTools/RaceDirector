using RaceDirector.Pipeline.Telemetry.V0;
using System.Text.Json;
using System.IO;
using System;
using RaceDirector.Plugin.HUD.Utils;
using static RaceDirector.Pipeline.Telemetry.V0.RaceDuration;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using System.Linq;
using System.Text;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public static class R3EDashTransformer
    {
        private static readonly UInt32 MajorVersion = 2;
        private static readonly UInt32 MinorVersion = 11;

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

                w.WriteNumber("GameInMenus", MatchAsInt32(gt.GameState, GameState.Menu));
                w.WriteNumber("GameInReplay", MatchAsInt32(gt.GameState, GameState.Replay));

                w.WriteNumber("GameUsingVr", ToInt32(gt.UsingVR));

                w.WriteObject("Player", _ =>
                {
                    // Player.GameSimulationTicks
                    // Player.GameSimulationTime

                    w.WriteObject("Position", _ =>
                    {
                        w.WriteCoordinates(gt.Player?.CgLocation, d => d.M);
                    });

                    // Player.Velocity.X
                    // Player.Velocity.Y
                    // Player.Velocity.Z
                    // Player.LocalVelocity.X
                    // Player.LocalVelocity.Y
                    // Player.LocalVelocity.Z
                    // Player.Acceleration.X
                    // Player.Acceleration.Y
                    // Player.Acceleration.Z

                    w.WriteObject("LocalAcceleration", _ =>
                    {
                        w.WriteCoordinates(gt.Player?.LocalAcceleration, a => a.MPS2);
                    });

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

                    w.WriteObject("LocalGforce", _ =>
                    {
                        w.WriteCoordinates(gt.Player?.LocalAcceleration, a => a.G);
                    });

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

                w.WriteNumber("LayoutLength", (gt.Event?.Track.Length?.M) ?? -1.0);

                w.WriteObject("SectorStartFactors", _ =>
                {
                    w.WriteSectors(gt.Event?.Track.SectorsEnd, st => st.Fraction);
                });

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
                w.WriteNumber("SessionPitSpeedLimit", gt.Session?.PitSpeedLimit.MPS ?? -1.0);
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
                w.WriteNumber("SessionTimeDuration", gt.Session?.Length switch
                {
                    TimeDuration length => length.Time.TotalSeconds,
                    TimePlusLapsDuration length => length.Time.TotalSeconds,
                    _ => -1.0
                });
                w.WriteNumber("SessionTimeRemaining", gt.Session?.Length switch
                {
                    TimeDuration length => length.Time.TotalSeconds - gt.Session.ElapsedTime.TotalSeconds,
                    TimePlusLapsDuration length => length.Time.TotalSeconds - gt.Session.ElapsedTime.TotalSeconds,
                    _ => -1.0
                });
                // MaxIncidentPoints

                w.WriteNumber("PitWindowStatus", PitWindowStatusAsInt32(gt));
                w.WriteNumber("PitWindowStart", gt.Session?.Requirements.PitWindow?.Start switch {
                    LapsDuration length => Convert.ToInt32(length.Laps),
                    TimeDuration length => Convert.ToInt32(length.Time.TotalMinutes),
                    _ => -1
                });
                w.WriteNumber("PitWindowEnd", gt.Session?.Requirements.PitWindow?.Finish switch
                {
                    LapsDuration length => Convert.ToInt32(length.Laps),
                    TimeDuration length => Convert.ToInt32(length.Time.TotalMinutes),
                    _ => -1
                });
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
                w.WriteNumber("PitTotalDuration", gt.FocusedVehicle?.Pit.PitLaneTime?.TotalSeconds ?? -1.0);
                w.WriteNumber("PitElapsedTime", gt.FocusedVehicle?.Pit.PitStallTime?.TotalSeconds ?? -1.0);
                w.WriteNumber("PitAction", PitActionAsInt32(gt.Player));

                // NumPitstopsPerformed
                // PitMinDurationTotal
                // PitMinDurationLeft

                w.WriteObject("Flags", _ =>
                {
                    w.WriteNumber("Yellow", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.Yellow)));

                    // Flags.YellowCausedIt
                    // Flags.YellowOvertake
                    // Flags.YellowPositionsGained
                    // Flags.SectorYellow.Sector1
                    // Flags.SectorYellow.Sector2
                    // Flags.SectorYellow.Sector3
                    // Flags.ClosestYellowDistanceIntoTrack

                    w.WriteNumber("Blue", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.Blue)));
                    w.WriteNumber("Black", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.Black)));
                    w.WriteNumber("Green", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.Green)));
                    w.WriteNumber("Checkered", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.Checkered)));
                    w.WriteNumber("White", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.White)));
                    w.WriteNumber("BlackAndWhite", ToInt32(gt.Player?.GameFlags.HasFlag(Flags.BlackAndWhite)));
                });

                // Position

                w.WriteNumber("PositionClass", ToInt32(gt.FocusedVehicle?.PositionClass));

                // FinishStatus
                // CutTrackWarnings

                w.WriteObject("Penalties", _ =>
                {
                    w.WriteNumber("DriveThrough", ToInt32(gt.FocusedVehicle?.Penalties.HasFlag(Penalties.DriveThrough)));
                    w.WriteNumber("StopAndGo", ToInt32(gt.FocusedVehicle?.Penalties.HasFlag(Penalties.StopAndGo)));
                    w.WriteNumber("PitStop", ToInt32(gt.FocusedVehicle?.Penalties.HasFlag(Penalties.PitStop)));
                    w.WriteNumber("TimeDeduction", ToInt32(gt.FocusedVehicle?.Penalties.HasFlag(Penalties.TimeDeduction)));
                    w.WriteNumber("SlowDown", ToInt32(gt.FocusedVehicle?.Penalties.HasFlag(Penalties.SlowDown)));
                });

                // NumPenalties

                w.WriteNumber("CompletedLaps", ToInt32(gt.FocusedVehicle?.CompletedLaps));
                w.WriteNumber("CurrentLapValid", ToInt32(gt.FocusedVehicle?.CurrentLapValid));

                // TrackSector

                w.WriteNumber("LapDistance", gt.FocusedVehicle?.CurrentLapDistance.Value.M ?? -1.0);
                w.WriteNumber("LapDistanceFraction", gt.FocusedVehicle?.CurrentLapDistance.Fraction ?? -1.0);

                // LapTimeBestLeader
                // LapTimeBestLeaderClass
                // SectorTimesSessionBestLap.Sector1
                // SectorTimesSessionBestLap.Sector2
                // SectorTimesSessionBestLap.Sector3

                w.WriteNumber("LapTimeBestSelf", gt.FocusedVehicle?.BestLapTime?.Overall.TotalSeconds ?? -1.0);
                w.WriteObject("SectorTimesBestSelf", _ =>
                {
                    w.WriteSectors(gt.FocusedVehicle?.BestLapTime?.Sectors.Cumulative, st => st.TotalSeconds);
                });

                // LapTimePreviousSelf
                // SectorTimesPreviousSelf.Sector1
                // SectorTimesPreviousSelf.Sector2
                // SectorTimesPreviousSelf.Sector3

                w.WriteNumber("LapTimeCurrentSelf", gt.FocusedVehicle?.CurrentLapTime?.Overall.TotalSeconds ?? -1.0);
                w.WriteObject("SectorTimesCurrentSelf", _ =>
                {
                    w.WriteSectors(gt.FocusedVehicle?.CurrentLapTime?.Sectors.Cumulative, st => st.TotalSeconds);
                });

                // LapTimeDeltaLeader
                // LapTimeDeltaLeaderClass
                // TimeDeltaFront => Player or -1.0
                // TimeDeltaBehind => Player or -1.0

                w.WriteNumber("TimeDeltaBestSelf", gt.Player?.PersonalBestDelta?.TotalSeconds ?? -1000.0);

                w.WriteObject("BestIndividualSectorTimeSelf", _ =>
                {
                    w.WriteSectors(gt.Player?.PersonalBestSectors?.Individual, st => st.TotalSeconds);
                });

                // BestIndividualSectorTimeLeader.Sector1
                // BestIndividualSectorTimeLeader.Sector2
                // BestIndividualSectorTimeLeader.Sector3

                w.WriteObject("BestIndividualSectorTimeLeaderClass", _ =>
                {
                    w.WriteSectors(gt.Player?.ClassBestSectors?.Individual, st => st.TotalSeconds);
                });

                // IncidentPoints
                // VehicleInfo.Name
                // VehicleInfo.CarNumber
                // VehicleInfo.ClassId
                // VehicleInfo.ModelId
                // VehicleInfo.TeamId
                // VehicleInfo.LiveryId
                // VehicleInfo.ManufacturerId
                // VehicleInfo.UserId

                w.WriteObject("VehicleInfo", _ =>
                {
                    w.WriteNumber("SlotId", ToInt32(gt.FocusedVehicle?.Id));
                    w.WriteNumber("ClassPerformanceIndex", gt.FocusedVehicle?.ClassPerformanceIndex ?? -1);
                    w.WriteNumber("EngineType", gt.FocusedVehicle?.EngineType switch {
                        EngineType.Combustion => 0,
                        EngineType.Electric => 1,
                        EngineType.Hybrid => 2,
                        _ => -1
                    });
                });

                // NOTE it is the current vehicle's driver name rather than player name!
                w.WriteString("PlayerName", ToBase64(gt.FocusedVehicle?.DriverName));

                w.WriteNumber("ControlType", gt.FocusedVehicle?.ControlType switch
                {
                    ControlType.LocalPlayer => 0,
                    ControlType.AI => 1,
                    ControlType.RemotePlayer => 2,
                    ControlType.Replay => 3,
                    _ => -1
                });

                w.WriteNumber("CarSpeed", gt.FocusedVehicle?.Speed.MPS ?? -1.0);
                
                w.WriteNumber("EngineRps", gt.Player?.Engine.Speed.RadPS ?? -1.0);
                w.WriteNumber("MaxEngineRps", gt.Player?.Engine.MaxSpeed.RadPS ?? -1.0);
                w.WriteNumber("UpshiftRps", gt.Player?.Engine.UpshiftSpeed.RadPS ?? -1.0);

                // Gear
                // NumGears

                w.WriteObject("CarCgLocation", _ =>
                {
                    w.WriteCoordinates(gt.FocusedVehicle?.Location, d => d.M);
                });

                w.WriteObject("CarOrientation", _ =>
                {
                    w.WriteOrientationPYR(gt.FocusedVehicle?.Orientation, a => a.Rad);
                });

                // LocalAcceleration.X
                // LocalAcceleration.Y
                // LocalAcceleration.Z
                // TotalMass

                w.WriteNumber("FuelLeft", gt.Player?.Fuel.Left ?? -1.0);
                w.WriteNumber("FuelCapacity", gt.Player?.Fuel.Max ?? -1.0);
                w.WriteNumber("FuelPerLap", gt.Player?.Fuel.PerLap ?? -1.0);

                // EngineWaterTemp
                // EngineOilTemp
                // FuelPressure
                // EngineOilPressure
                // TurboPressure

                w.WriteNumber("Throttle", gt.FocusedVehicle?.Inputs?.Throttle ?? -1.0);
                w.WriteNumber("ThrottleRaw", gt.Player?.RawInputs.Throttle ?? -1.0);
                w.WriteNumber("Brake", gt.FocusedVehicle?.Inputs?.Brake ?? -1.0);
                w.WriteNumber("BrakeRaw", gt.Player?.RawInputs.Brake ?? -1.0);
                w.WriteNumber("Clutch", gt.FocusedVehicle?.Inputs?.Clutch ?? -1.0);
                w.WriteNumber("ClutchRaw", gt.Player?.RawInputs.Clutch ?? -1.0);
                w.WriteNumber("SteerInputRaw", gt.Player?.RawInputs.Steering ?? 0.0);

                // SteerLockDegrees

                w.WriteNumber("SteerWheelRangeDegrees", ToUInt32(gt.Player?.RawInputs.SteerWheelRange.Deg));

                w.WriteObject("AidSettings", _ =>
                {
                    w.WriteNumber("Abs", ToInt32(gt.Player?.DrivingAids.Abs?.Level));
                    w.WriteNumber("Tc", ToInt32(gt.Player?.DrivingAids.Tc?.Level));
                    w.WriteNumber("Esp", ToInt32(gt.Player?.DrivingAids.Esp?.Level));
                    w.WriteNumber("Countersteer", ToInt32(gt.Player?.DrivingAids.Countersteer?.Level));
                    w.WriteNumber("Cornering", ToInt32(gt.Player?.DrivingAids.Cornering?.Level));
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
                    w.WriteNumber("EngagedTimeLeft", gt.Player?.PushToPass?.EngagedTimeLeft.TotalSeconds ?? -1.0); // not sure
                    w.WriteNumber("WaitTimeLeft", gt.Player?.PushToPass?.WaitTimeLeft.TotalSeconds ?? -1.0); // not sure
                });

                // BrakeBias

                w.WriteNumber("DrsNumActivationsTotal", ToInt32(gt.Player?.Drs?.ActivationsLeft?.Total));
                w.WriteNumber("PtPNumActivationsTotal", ToInt32(gt.Player?.PushToPass?.ActivationsLeft?.Total));

                // TireType
                // TireRps.FrontLeft
                // TireRps.FrontRight
                // TireRps.RearLeft
                // TireRps.RearRight
                // TireSpeed.FrontLeft
                // TireSpeed.FrontRight
                // TireSpeed.RearLeft
                // TireSpeed.RearRight

                w.WriteTyres("TireGrip", gt.Player?.Tyres, t => t.Grip, -1.0);
                w.WriteTyres("TireWear", gt.Player?.Tyres, t => t.Wear, -1.0);

                // TireFlatspot.FrontLeft
                // TireFlatspot.FrontRight
                // TireFlatspot.RearLeft
                // TireFlatspot.RearRight
                // TirePressure.FrontLeft
                // TirePressure.FrontRight
                // TirePressure.RearLeft
                // TirePressure.RearRight

                w.WriteTyres("TireDirt", gt.Player?.Tyres, t => t.Dirt, -1.0);

                // TODO
                // TireTemp.FrontLeft.CurrentTemp.Left
                // TireTemp.FrontLeft.CurrentTemp.Center
                // TireTemp.FrontLeft.CurrentTemp.Right
                // TireTemp.FrontLeft.OptimalTemp
                // TireTemp.FrontLeft.ColdTemp
                // TireTemp.FrontLeft.HotTemp
                // TireTemp.FrontRight.CurrentTemp.Left
                // TireTemp.FrontRight.CurrentTemp.Center
                // TireTemp.FrontRight.CurrentTemp.Right
                // TireTemp.FrontRight.OptimalTemp
                // TireTemp.FrontRight.ColdTemp
                // TireTemp.FrontRight.HotTemp
                // TireTemp.RearLeft.CurrentTemp.Left
                // TireTemp.RearLeft.CurrentTemp.Center
                // TireTemp.RearLeft.CurrentTemp.Right
                // TireTemp.RearLeft.OptimalTemp
                // TireTemp.RearLeft.ColdTemp
                // TireTemp.RearLeft.HotTemp
                // TireTemp.RearRight.CurrentTemp.Left
                // TireTemp.RearRight.CurrentTemp.Center
                // TireTemp.RearRight.CurrentTemp.Right
                // TireTemp.RearRight.OptimalTemp
                // TireTemp.RearRight.ColdTemp
                // TireTemp.RearRight.HotTemp

                // TireTypeFront
                // TireTypeRear
                // TireSubtypeFront
                // TireSubtypeRear

                // TODO
                // BrakeTemp.FrontLeft.CurrentTemp
                // BrakeTemp.FrontLeft.OptimalTemp
                // BrakeTemp.FrontLeft.ColdTemp
                // BrakeTemp.FrontLeft.HotTemp
                // BrakeTemp.FrontRight.CurrentTemp
                // BrakeTemp.FrontRight.OptimalTemp
                // BrakeTemp.FrontRight.ColdTemp
                // BrakeTemp.FrontRight.HotTemp
                // BrakeTemp.RearLeft.CurrentTemp
                // BrakeTemp.RearLeft.OptimalTemp
                // BrakeTemp.RearLeft.ColdTemp
                // BrakeTemp.RearLeft.HotTemp
                // BrakeTemp.RearRight.CurrentTemp
                // BrakeTemp.RearRight.OptimalTemp
                // BrakeTemp.RearRight.ColdTemp
                // BrakeTemp.RearRight.HotTemp

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

                // TODO
                // CarDamage.Engine
                // CarDamage.Transmission
                // CarDamage.Aerodynamics
                // CarDamage.Suspension

                // NumCars

                // TODO
                // DriverData[].DriverInfo.Name

                // DriverData[].DriverInfo.CarNumber
                // DriverData[].DriverInfo.ClassId
                // DriverData[].DriverInfo.ModelId
                // DriverData[].DriverInfo.TeamId
                // DriverData[].DriverInfo.LiveryId
                // DriverData[].DriverInfo.ManufacturerId
                // DriverData[].DriverInfo.UserId

                // TODO
                // DriverData[].DriverInfo.SlotId
                // DriverData[].DriverInfo.ClassPerformanceIndex

                // DriverData[].DriverInfo.EngineType
                // DriverData[].FinishStatus
                // DriverData[].Place
                // DriverData[].TrackSector

                // TODO
                // DriverData[].PlaceClass
                // DriverData[].LapDistance
                // DriverData[].Position.X
                // DriverData[].Position.Y
                // DriverData[].Position.Z

                // DriverData[].TrackSector

                // TODO
                // DriverData[].CompletedLaps

                // DriverData[].CurrentLapValid
                // DriverData[].LapTimeCurrentSelf
                // DriverData[].SectorTimeCurrentSelf.Sector1
                // DriverData[].SectorTimeCurrentSelf.Sector2
                // DriverData[].SectorTimeCurrentSelf.Sector3
                // DriverData[].SectorTimePreviousSelf.Sector1
                // DriverData[].SectorTimePreviousSelf.Sector2
                // DriverData[].SectorTimePreviousSelf.Sector3

                // TODO
                // DriverData[].SectorTimeBestSelf.Sector1
                // DriverData[].SectorTimeBestSelf.Sector2
                // DriverData[].SectorTimeBestSelf.Sector3
                // DriverData[].TimeDeltaFront
                // DriverData[].TimeDeltaBehind

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

        //public static void WriteVector3<T, V>(this Utf8JsonWriter writer, Vector3<T> v, Func<T, V> f)
        //{

        //}

        private static void WriteSectors<T>(this Utf8JsonWriter writer, T[]? v, Func<T, Double> f)
        {
            for (int i = 0; i < 3; i++)
            {
                writer.WriteNumber("Sector" + (i + 1), v?.Length > i ? f(v[i]) : -1.0);
            }
        }

        private static void WriteCoordinates<T>(this Utf8JsonWriter writer, Vector3<T>? v, Func<T, Double> f)
        {
            writer.WriteNumber("X", v is not null ? f(v.X) : 0.0);
            writer.WriteNumber("Y", v is not null ? f(v.Y) : 0.0);
            writer.WriteNumber("Z", v is not null ? f(v.Z) : 0.0);
        }


        private static void WriteOrientationPYR(this Utf8JsonWriter writer, Orientation? v, Func<IAngle, Double> f)
        {
            writer.WriteNumber("Pitch", v is not null ? f(v.Pitch) : 0.0);
            writer.WriteNumber("Yaw", v is not null ? f(v.Yaw) : 0.0);
            writer.WriteNumber("Roll", v is not null ? f(v.Roll) : 0.0);
        }

        private static void WriteOrientationXYZ(this Utf8JsonWriter writer, Orientation? v, Func<IAngle, Double> f)
        {
            writer.WriteNumber("X", v is not null ? f(v.Pitch) : 0.0);
            writer.WriteNumber("Y", v is not null ? f(v.Yaw) : 0.0);
            writer.WriteNumber("Z", v is not null ? f(v.Roll) : 0.0);
        }

        private static void WriteTyres(this Utf8JsonWriter writer, String key, ITyre[][]? maybeTyres, Func<ITyre, Double> f, Double defaultValue)
        {
            var tyres = maybeTyres ?? Array.Empty<ITyre[]>();
            writer.WriteObject(key, _ => {
                writer.WriteTyre("FrontLeft", tyres, 0, 0, f, defaultValue);
                writer.WriteTyre("FrontRight", tyres, 0, 1, f, defaultValue);
                writer.WriteTyre("RearLeft", tyres, 1, 0, f, defaultValue);
                writer.WriteTyre("RearRight", tyres, 1, 1, f, defaultValue);
            });
        }

        private static void WriteTyre(this Utf8JsonWriter writer, String tyreName, ITyre[][] tyres, int i, int j, Func<ITyre, Double> f, Double defaultValue)
        {
            writer.WriteNumber(tyreName, (i < tyres.Length && j < tyres[i].Length) ? f(tyres[i][j]) : defaultValue);
        }


        private static String ToBase64(String? value)
        {
            if (value is null)
                return "AA==";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        private static UInt32 ToUInt32(Double? value)
        {
            try
            {
                return Convert.ToUInt32(value);
            } catch
            {
                return 0;
            }
        }

        private static Int32 ToInt32(UInt32? value)
        {
            if (value is null || value > Int32.MaxValue)
                return -1;
            return Convert.ToInt32(value);
        }

        private static Int32 ToInt32(Boolean? value)
        {
            return MatchAsInt32(value, true);
        }

        private static Int32 MatchAsInt32<T>(T? value, T constant)
        {
            if (value is null)
                return -1;
            return value.Equals(constant) ? 1 : 0;
        }

        private static Int32 ToInt32(IStartLights? startLights)
        {
            if (startLights is null)
                return -1;
            else if (startLights.Colour == LightColour.Green)
                return 6;
            else
                return Convert.ToInt32(5 * startLights.Lit.Value / startLights.Lit.Total);
        }


        private static Int32 PitWindowStatusAsInt32(IGameTelemetry gt)
        {
            if (gt.Session is null)
                return -1; // Unavailable
            var pitWindow = gt.Session.Requirements.PitWindow;
            if (pitWindow is null)
                return 0; // Disabled
            var vehicle = gt.FocusedVehicle;
            if (vehicle?.Pit.MandatoryStopsDone > 0)
                return 4; // Completed

            switch (pitWindow)
            {
                case Interval<IPitWindowBoundary>(LapsDuration start, LapsDuration finish):
                    if (vehicle is null)
                        return -1; // Unavailable
                    if (start.Laps.CompareTo(vehicle.CompletedLaps) > 0
                        || 0 > finish.Laps.CompareTo(vehicle.CompletedLaps))
                        return 1; // Closed
                    break;
                case Interval<IPitWindowBoundary>(TimeDuration start, TimeDuration finish):
                    if (start.Time.CompareTo(gt.Session.ElapsedTime) > 0
                        || 0 > finish.Time.CompareTo(gt.Session.ElapsedTime))
                        return 1; // Closed
                    break;
            }

            if (vehicle?.Pit.PitLaneState == PitLaneState.Stopped)
                return 3; // Stopped
            else
                return 2; // Open
        }

        private static Int32 InPitLaneAsInt32(IVehicle? vehicle)
        {
            if (vehicle is null)
                return -1;
            if (vehicle.Pit.PitLaneState is null)
                return 0;
            return 1;
        }

        private static Int32 PitStateAsInt32(IGameTelemetry gt)
        {
            if (gt.FocusedVehicle is null)
                return -1;
            switch (gt.FocusedVehicle?.Pit.PitLaneState)
            {
                case PitLaneState.Entered:
                    return 2;
                case PitLaneState.Stopped:
                    return 3;
                case PitLaneState.Exiting:
                    return 4;
            }
            if (gt.Player?.PitStop.HasFlag(PlayerPitStop.Requested) ?? false)
                return 1;
            return 0;
        }

        private static Int32 PitActionAsInt32(IPlayer? player)
        {
            if (player is null)
                return -1;
            Int32 pitAction = 0;
            foreach ((PlayerPitStop playerPitstopFlag, Int32 pitActionFlag) in pitActionMapping)
                if (player.PitStop.HasFlag(playerPitstopFlag)) pitAction += pitActionFlag;
            return pitAction;
        }

        private static readonly (PlayerPitStop, Int32)[] pitActionMapping = {
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
    }
}
