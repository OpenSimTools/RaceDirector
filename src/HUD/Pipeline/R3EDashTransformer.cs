using RaceDirector.Pipeline.Telemetry.V0;
using System.Text.Json;
using System.IO;
using System;
using RaceDirector.Plugin.HUD.Utils;
using static RaceDirector.Pipeline.Telemetry.V0.RaceDuration;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public static class R3EDashTransformer
    {
        private static readonly UInt32 MajorVersion = 2;
        private static readonly UInt32 MinorVersion = 11;

        private static readonly UInt32 NumSectors = 3;

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

                w.WriteNumber("GameInMenus", MatchAsNumber(gt.GameState, GameState.Menu));
                w.WriteNumber("GameInReplay", MatchAsNumber(gt.GameState, GameState.Replay));

                w.WriteNumber("GameUsingVr", BooleanAsNumber(gt.UsingVR));

                w.WriteObject("Player", _ =>
                {
                    // Player.GameSimulationTicks
                    // Player.GameSimulationTime

                    w.WriteObject("Position", _ =>
                    {
                        w.WriteCoords(gt.Player?.CgLocation, p => p.M);
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
                        w.WriteCoords(gt.Player?.LocalAcceleration, a => a.MPS2);
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
                        w.WriteCoords(gt.Player?.LocalAcceleration, a => a.G);
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
                w.WriteNumber("StartLights", StartLightsAsNumber(gt.Session?.StartLights));

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

                w.WriteNumber("PitWindowStatus", PitWindowStatusAsNumber(gt));
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
                w.WriteNumber("InPitLane", InPitLaneAsNumber(gt.CurrentVehicle));

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

                w.WriteNumber("PitState", PitStateAsNumber(gt));
                w.WriteNumber("PitTotalDuration", gt.CurrentVehicle?.Pit.PitLaneTime?.TotalSeconds ?? -1.0);
                w.WriteNumber("PitElapsedTime", gt.CurrentVehicle?.Pit.PitStallTime?.TotalSeconds ?? -1.0);
                w.WriteNumber("PitAction", PitActionAsNumber(gt.Player));

                // NumPitstopsPerformed
                // PitMinDurationTotal
                // PitMinDurationLeft

                w.WriteObject("Flags", _ =>
                {
                    w.WriteNumber("Yellow", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.Yellow)));

                    // Flags.YellowCausedIt
                    // Flags.YellowOvertake
                    // Flags.YellowPositionsGained
                    // Flags.SectorYellow.Sector1
                    // Flags.SectorYellow.Sector2
                    // Flags.SectorYellow.Sector3
                    // Flags.ClosestYellowDistanceIntoTrack

                    w.WriteNumber("Blue", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.Blue)));
                    w.WriteNumber("Black", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.Black)));
                    w.WriteNumber("Green", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.Green)));
                    w.WriteNumber("Checkered", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.Checkered)));
                    w.WriteNumber("White", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.White)));
                    w.WriteNumber("BlackAndWhite", BooleanAsNumber(gt.Player?.GameFlags.HasFlag(Flags.BlackAndWhite)));
                });

                // Position

                w.WriteNumber("PositionClass", UInt32AsNumber(gt.CurrentVehicle?.PositionClass));

                // FinishStatus
                // CutTrackWarnings

                w.WriteObject("Penalties", _ =>
                {
                    w.WriteNumber("DriveThrough", BooleanAsNumber(gt.CurrentVehicle?.Penalties.HasFlag(Penalties.DriveThrough)));
                    w.WriteNumber("StopAndGo", BooleanAsNumber(gt.CurrentVehicle?.Penalties.HasFlag(Penalties.StopAndGo)));
                    w.WriteNumber("PitStop", BooleanAsNumber(gt.CurrentVehicle?.Penalties.HasFlag(Penalties.PitStop)));
                    w.WriteNumber("TimeDeduction", BooleanAsNumber(gt.CurrentVehicle?.Penalties.HasFlag(Penalties.TimeDeduction)));
                    w.WriteNumber("SlowDown", BooleanAsNumber(gt.CurrentVehicle?.Penalties.HasFlag(Penalties.SlowDown)));
                });

                // NumPenalties

                w.WriteNumber("CompletedLaps", UInt32AsNumber(gt.CurrentVehicle?.CompletedLaps));
                w.WriteNumber("CurrentLapValid", BooleanAsNumber(gt.CurrentVehicle?.CurrentLapValid));

                // TrackSector

                w.WriteNumber("LapDistance", gt.CurrentVehicle?.CurrentLapDistance.Value.M ?? -1.0);
                w.WriteNumber("LapDistanceFraction", gt.CurrentVehicle?.CurrentLapDistance.Fraction ?? -1.0);

                // LapTimeBestLeader
                // LapTimeBestLeaderClass
                // SectorTimesSessionBestLap.Sector1
                // SectorTimesSessionBestLap.Sector2
                // SectorTimesSessionBestLap.Sector3

                w.WriteNumber("LapTimeBestSelf", gt.CurrentVehicle?.BestLapTime?.Overall.TotalSeconds ?? -1.0);
                w.WriteObject("SectorTimesBestSelf", _ =>
                {
                    w.WriteSectors(gt.CurrentVehicle?.BestLapTime?.Sectors.Cumulative, st => st.TotalSeconds);
                });

                // LapTimePreviousSelf
                // SectorTimesPreviousSelf.Sector1
                // SectorTimesPreviousSelf.Sector2
                // SectorTimesPreviousSelf.Sector3

                w.WriteNumber("LapTimeCurrentSelf", gt.CurrentVehicle?.CurrentLapTime?.Overall.TotalSeconds ?? -1.0);
                w.WriteObject("SectorTimesCurrentSelf", _ =>
                {
                    w.WriteSectors(gt.CurrentVehicle?.CurrentLapTime?.Sectors.Cumulative, st => st.TotalSeconds);
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
                    w.WriteNumber("SlotId", UInt32AsNumber(gt.CurrentVehicle?.Id));
                    w.WriteNumber("ClassPerformanceIndex", gt.CurrentVehicle?.ClassPerformanceIndex ?? -1);
                    w.WriteNumber("EngineType", gt.CurrentVehicle?.EngineType switch {
                        EngineType.Combustion => 0,
                        EngineType.Electric => 1,
                        EngineType.Hybrid => 2,
                        _ => -1
                    });
                });

                // TODO
                // PlayerName - NOTE it is the current vehicle's driver name rather than player name!
                // ControlType
                // CarSpeed
                // EngineRps
                // MaxEngineRps
                // UpshiftRps

                // Gear
                // NumGears

                // TODO
                // CarCgLocation.X
                // CarCgLocation.Y
                // CarCgLocation.Z
                // CarOrientation.Pitch
                // CarOrientation.Yaw
                // CarOrientation.Roll

                // TODO
                // FuelLeft
                // FuelCapacity
                // FuelPerLap

                // EngineWaterTemp
                // EngineOilTemp
                // FuelPressure
                // EngineOilPressure
                // TurboPressure

                // TODO
                // Throttle
                // ThrottleRaw
                // Brake
                // BrakeRaw
                // Clutch
                // ClutchRaw
                // SteerInputRaw

                // SteerLockDegrees

                // TODO
                // SteerWheelRangeDegrees
                // AidSettings.Abs
                // AidSettings.Tc
                // AidSettings.Esp
                // AidSettings.Countersteer
                // AidSettings.Cornering
                // Drs.Equipped
                // Drs.Available
                // Drs.NumActivationsLeft
                // Drs.Engaged

                // PitLimiter

                // TODO
                // PushToPass.Available
                // PushToPass.Engaged
                // PushToPass.AmountLeft
                // PushToPass.EngagedTimeLeft
                // PushToPass.WaitTimeLeft

                // BrakeBias
                // DrsNumActivationsTotal
                // PtPNumActivationsTotal
                // TireType
                // TireRps.FrontLeft
                // TireRps.FrontRight
                // TireRps.RearLeft
                // TireRps.RearRight
                // TireSpeed.FrontLeft
                // TireSpeed.FrontRight
                // TireSpeed.RearLeft
                // TireSpeed.RearRight

                // TODO
                // TireGrip.FrontLeft
                // TireGrip.FrontRight
                // TireGrip.RearLeft
                // TireGrip.RearRight
                // TireWear.FrontLeft
                // TireWear.FrontRight
                // TireWear.RearLeft
                // TireWear.RearRight


                // TireFlatspot.FrontLeft
                // TireFlatspot.FrontRight
                // TireFlatspot.RearLeft
                // TireFlatspot.RearRight
                // TirePressure.FrontLeft
                // TirePressure.FrontRight
                // TirePressure.RearLeft
                // TirePressure.RearRight

                // TODO
                // TireDirt.FrontLeft
                // TireDirt.FrontRight
                // TireDirt.RearLeft
                // TireDirt.RearRight
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

        public static void WriteSectors<T>(this Utf8JsonWriter writer, T[]? v, Func<T, Double> f)
        {
            for (int i = 0; i < NumSectors; i++)
            {
                writer.WriteNumber("Sector" + (i + 1), v?.Length > i ? f(v[i]) : -1.0);
            }
        }

        public static void WriteCoords<T>(this Utf8JsonWriter writer, Vector3<T>? v, Func<T, Double> f)
        {
            writer.WriteNumber("X", v is not null ? f(v.X) : 0.0);
            writer.WriteNumber("Y", v is not null ? f(v.Y) : 0.0);
            writer.WriteNumber("Z", v is not null ? f(v.Z) : 0.0);
        }

        private static Int32 UInt32AsNumber(UInt32? value)
        {
            if (value is null || value > Int32.MaxValue)
                return -1;
            return Convert.ToInt32(value);
        }

        private static Int32 BooleanAsNumber(Boolean? value)
        {
            return MatchAsNumber(value, true);
        }

        private static Int32 MatchAsNumber<T>(T? value, T constant)
        {
            if (value is null)
                return -1;
            return value.Equals(constant) ? 1 : 0;
        }

        private static Int32 StartLightsAsNumber(IStartLights? startLights)
        {
            if (startLights is null)
                return -1;
            else if (startLights.Colour == LightColour.Green)
                return 6;
            else
                return Convert.ToInt32(5 * startLights.Lit.Value / startLights.Lit.Total);
        }


        private static Int32 PitWindowStatusAsNumber(IGameTelemetry gt)
        {
            if (gt.Session is null)
                return -1; // Unavailable
            var pitWindow = gt.Session.Requirements.PitWindow;
            if (pitWindow is null)
                return 0; // Disabled
            var vehicle = gt.CurrentVehicle;
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

        private static Int32 InPitLaneAsNumber(IVehicle? vehicle)
        {
            if (vehicle is null)
                return -1;
            if (vehicle.Pit.PitLaneState is null)
                return 0;
            return 1;
        }

        private static decimal PitStateAsNumber(IGameTelemetry gt)
        {
            if (gt.CurrentVehicle is null)
                return -1;
            switch (gt.CurrentVehicle.Pit.PitLaneState)
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

        private static Int32 PitActionAsNumber(IPlayer? player)
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
