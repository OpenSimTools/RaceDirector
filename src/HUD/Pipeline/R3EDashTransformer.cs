﻿using RaceDirector.Pipeline.Telemetry.V0;
using System.Text.Json;
using System.IO;
using System;
using RaceDirector.Plugin.HUD.Utils;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public static class R3EDashTransformer
    {
        private static readonly Double UndefinedDoubleValue = -1.0;

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
                w.WriteNumber("VersionMajor", 2);
                w.WriteNumber("VersionMinor", 10);

                w.WriteNumber("GameInMenus", MatchToInteger(telemetry.GameState, GameState.Menu));
                w.WriteNumber("GameInReplay", MatchToInteger(telemetry.GameState, GameState.Replay));
                w.WriteNumber("GameUsingVr", BooleanToInteger(telemetry.UsingVR));

                //w.WriteObject("Player", _ =>
                //{
                //    w.WriteObject("Position", _ =>
                //    {
                //        // TODO
                //        //w.WriteNumber("X", telemetry.Player.???);
                //        //w.WriteNumber("Z", telemetry.Player.???);
                //    });
                //});

                w.WriteNumber("LayoutLength", (telemetry.Event?.Track.Length?.M) ?? UndefinedDoubleValue);
                w.WriteObject("SectorStartFactors", _ =>
                {
                    var sectorsEnd = telemetry.Event?.Track.SectorsEnd;
                    if (sectorsEnd?.Length == 3)
                    {
                        w.WriteNumber("Sector1", sectorsEnd[0].Fraction);
                        w.WriteNumber("Sector2", sectorsEnd[1].Fraction);
                        w.WriteNumber("Sector3", sectorsEnd[2].Fraction);
                    }
                    else
                    {
                        w.WriteNumber("Sector1", UndefinedDoubleValue);
                        w.WriteNumber("Sector2", UndefinedDoubleValue);
                        w.WriteNumber("Sector3", UndefinedDoubleValue);
                    }
                });

                //w.WriteNumber("SessionType", telemetry.Session?.Type switch
                //{
                //    SessionType.Practice => 0,
                //    SessionType.Qualify => 1,
                //    SessionType.Race => 2,
                //    SessionType.Warmup => 3,
                //    _ => -1
                //});
                // SessionPitSpeedLimit
                // SessionPhase
                // StartLights

                //w.WriteNumber("FuelUseActive", Convert.ToInt32(telemetry.Event?.FuelRate ?? -1.0));

                // NumberOfLaps
                // SessionTimeRemaining
                // PitWindowStatus
                // PitWindowStart
                // PitWindowEnd
                // InPitlane

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
