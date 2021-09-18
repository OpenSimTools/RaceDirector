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


                // NOTE ** PlayerName is the current vehicle's driver name rather than player name!
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
