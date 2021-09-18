using RaceDirector.Pipeline.Telemetry.V0;
using System.Text.Json;
using System.IO;
using System;
using RaceDirector.Plugin.HUD.Utils;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public static class R3EDashTransformer
    {
        private static readonly UInt32 MajorVersion = 2;
        private static readonly UInt32 MinorVersion = 10;
        private static readonly Double UndefinedDoubleValue = -1.0;
        private static readonly Int32 UndefinedIntegerValue = -1;
        private static readonly String UndefinedBase64 = "AA==";
        private static readonly Int32 UndefinedGear = -2;

        private static readonly JsonWriterOptions JsonWriterOptions = new JsonWriterOptions();

        public static byte[] ToR3EDash(IGameTelemetry telemetry)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, JsonWriterOptions))
                {
                    WriteR3EDash(writer, telemetry);
                }
                return stream.ToArray();
            }
        }

        private static void WriteR3EDash(Utf8JsonWriter w, IGameTelemetry telemetry)
        {
            w.WriteObject(_ =>
            {
                w.WriteNumber("VersionMajor", MajorVersion);
                w.WriteNumber("VersionMinor", MinorVersion);

                w.WriteNumber("GameInMenus", MatchToInteger(telemetry.GameState, GameState.Menu));
                w.WriteNumber("GameInReplay", MatchToInteger(telemetry.GameState, GameState.Replay));
                w.WriteNumber("GameUsingVr", BooleanToInteger(telemetry.UsingVR));

                w.WriteObject("Player", _ =>
                {
                    w.WriteObject("Position", _ =>
                    {
                        w.WriteNumber("X", (telemetry.Player?.CgLocation.X.M) ?? 0.0);
                        w.WriteNumber("Y", (telemetry.Player?.CgLocation.Y.M) ?? 0.0);
                        w.WriteNumber("Z", (telemetry.Player?.CgLocation.Z.M) ?? 0.0);
                    });
                    // Player.LocalGforce.X
                    // Player.LocalGforce.Y
                    // Player.LocalGforce.Z
                });

                w.WriteNumber("LayoutLength", (telemetry.Event?.Track.Length?.M) ?? UndefinedDoubleValue);
                w.WriteObject("SectorStartFactors", _ =>
                {
                    var sectorsEnd = telemetry.Event?.Track.SectorsEnd;
                    for (int i = 0; i < 3; i++)
                    {
                        w.WriteNumber("Sector" + (i + 1), sectorsEnd?.Length > i ? sectorsEnd[i].Fraction : UndefinedDoubleValue);
                    }
                });

                w.WriteNumber("SessionType", telemetry.Session?.Type switch
                {
                    SessionType.Practice => 0,
                    SessionType.Qualify => 1,
                    SessionType.Race => 2,
                    SessionType.Warmup => 3,
                    _ => -1
                });

                // SessionPitSpeedLimit
                // SessionPhase
                // StartLights

                w.WriteNumber("FuelUseActive", Convert.ToInt32(telemetry.Event?.FuelRate ?? UndefinedIntegerValue));

                // NumberOfLaps
                // SessionTimeRemaining
                // PitWindowStatus
                // PitWindowStart
                // PitWindowEnd
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
    }
}
