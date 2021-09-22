using RaceDirector.Pipeline.Telemetry.V0;
using System.Text.Json;
using System.IO;
using System;
using RaceDirector.Plugin.HUD.Utils;
using static RaceDirector.Pipeline.Telemetry.V0.RaceDuration;
using RaceDirector.Pipeline.Telemetry;

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

                w.WriteNumber("GameInMenus", MatchToInteger(gt.GameState, GameState.Menu));
                w.WriteNumber("GameInReplay", MatchToInteger(gt.GameState, GameState.Replay));

                w.WriteNumber("GameUsingVr", BooleanToInteger(gt.UsingVR));

                w.WriteObject("Player", _ =>
                {
                    // Player.GameSimulationTicks
                    // Player.GameSimulationTime

                    w.WriteObject("Position", _ =>
                    {
                        w.WriteNumber("X", (gt.Player?.CgLocation.X.M) ?? 0.0);
                        w.WriteNumber("Y", (gt.Player?.CgLocation.Y.M) ?? 0.0);
                        w.WriteNumber("Z", (gt.Player?.CgLocation.Z.M) ?? 0.0);
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
                        w.WriteNumber("X", (gt.Player?.LocalAcceleration.X.MPS2) ?? 0.0);
                        w.WriteNumber("Y", (gt.Player?.LocalAcceleration.Y.MPS2) ?? 0.0);
                        w.WriteNumber("Z", (gt.Player?.LocalAcceleration.Z.MPS2) ?? 0.0);
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
                        w.WriteNumber("X", (gt.Player?.LocalAcceleration.X.G) ?? 0.0);
                        w.WriteNumber("Y", (gt.Player?.LocalAcceleration.Y.G) ?? 0.0);
                        w.WriteNumber("Z", (gt.Player?.LocalAcceleration.Z.G) ?? 0.0);
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
                    var sectorsEnd = gt.Event?.Track.SectorsEnd;
                    for (int i = 0; i < 3; i++)
                    {
                        w.WriteNumber("Sector" + (i + 1), sectorsEnd?.Length > i ? sectorsEnd[i].Fraction : -1.0);
                    }
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
                    // TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

                // TODO
                // InPitlane
                // PitState
                // PitTotalDuration
                // PitElapsedTime
                // PitAction
                // Flags.Yellow
                // Flags.Blue
                // Flags.Black
                // Flags.Green
                // Flags.Checkered
                // Flags.White
                // Flags.BlackAndWhite
                // PositionClass
                // Penalties.DriveThrough
                // Penalties.StopAndGo
                // Penalties.PitStop
                // Penalties.TimeDeduction
                // Penalties.SlowDown
                // CompletedLaps
                // CurrentLapValid
                // LapDistance
                // LapDistanceFraction
                // LapTimeBestSelf
                // SectorTimesBestSelf.Sector1
                // SectorTimesBestSelf.Sector2
                // SectorTimesBestSelf.Sector3
                // LapTimeCurrentSelf
                // SectorTimesCurrentSelf.Sector1
                // SectorTimesCurrentSelf.Sector2
                // SectorTimesCurrentSelf.Sector3
                // TimeDeltaBestSelf
                // BestIndividualSectorTimeSelf.Sector1
                // BestIndividualSectorTimeSelf.Sector2
                // BestIndividualSectorTimeSelf.Sector3
                // BestIndividualSectorTimeLeaderClass.Sector1
                // BestIndividualSectorTimeLeaderClass.Sector2
                // BestIndividualSectorTimeLeaderClass.Sector3
                // VehicleInfo.SlotId
                // VehicleInfo.ClassPerformanceIndex
                // VehicleInfo.EngineType
                // PlayerName - NOTE it is the current vehicle's driver name rather than player name!
                // ControlType
                // CarSpeed
                // EngineRps
                // MaxEngineRps
                // UpshiftRps
                // CarCgLocation.X
                // CarCgLocation.Y
                // CarCgLocation.Z
                // CarOrientation.Pitch
                // CarOrientation.Yaw
                // CarOrientation.Roll
                // FuelLeft
                // FuelCapacity
                // FuelPerLap
                // Throttle
                // ThrottleRaw
                // Brake
                // BrakeRaw
                // Clutch
                // ClutchRaw
                // SteerInputRaw
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
                // PushToPass.Available
                // PushToPass.Engaged
                // PushToPass.AmountLeft
                // PushToPass.EngagedTimeLeft
                // PushToPass.WaitTimeLeft
                // TireGrip.FrontLeft
                // TireGrip.FrontRight
                // TireGrip.RearLeft
                // TireGrip.RearRight
                // TireWear.FrontLeft
                // TireWear.FrontRight
                // TireWear.RearLeft
                // TireWear.RearRight
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
                // CarDamage.Engine
                // CarDamage.Transmission
                // CarDamage.Aerodynamics
                // CarDamage.Suspension
                // DriverData[].DriverInfo.Name
                // DriverData[].DriverInfo.SlotId
                // DriverData[].DriverInfo.ClassPerformanceIndex
                // DriverData[].PlaceClass
                // DriverData[].LapDistance
                // DriverData[].Position.X
                // DriverData[].Position.Y
                // DriverData[].Position.Z
                // DriverData[].CompletedLaps
                // DriverData[].SectorTimeBestSelf.Sector1
                // DriverData[].SectorTimeBestSelf.Sector2
                // DriverData[].SectorTimeBestSelf.Sector3
                // DriverData[].TimeDeltaFront
                // DriverData[].TimeDeltaBehind
            });
        }

        private static Int32 BooleanToInteger(Boolean? value)
        {
            return MatchToInteger(value, true);
        }

        private static Int32 MatchToInteger<T>(T? value, T constant) // FIXME!
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
                        || 0 > finish.Laps.CompareTo(gt.CurrentVehicle.CompletedLaps))
                        return 1; // Closed
                    break;
                case Interval<IPitWindowBoundary>(TimeDuration start, TimeDuration finish):
                    if (start.Time.CompareTo(gt.Session.ElapsedTime) > 0
                        || 0 > finish.Time.CompareTo(gt.Session.ElapsedTime))
                        return 1; // Closed
                    break;
            }

            if (vehicle?.Pit.InPitStall == true)
                return 3; // Stopped
            else
                return 2; // Open
        }
    }
}
