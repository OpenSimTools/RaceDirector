using HUD.Tests.TestUtils;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Plugin.HUD.Pipeline;
using System.Text.Json;
using Xunit;
using Xunit.Categories;
using System;
using RaceDirector.Pipeline.Telemetry.Physics;
using AutoBogus;
using RaceDirector.Pipeline.Telemetry.V0;
using AutoBogus.Moq;
using static RaceDirector.Pipeline.Telemetry.V0.RaceDuration;

namespace HUD.Tests.Pipeline
{
    [UnitTest]
    public class R3EDashTransformerTest
    {
        private GameTelemetry gt = AutoFaker.Generate<GameTelemetry>(b => b.WithBinder<MoqBinder>());

        [Fact]
        public void VersionInformation()
        {
            var result = ToR3EDash(gt);

            Assert.Equal(2, result.Path("VersionMajor").GetInt32());
            Assert.Equal(11, result.Path("VersionMinor").GetInt32());
        }

        [Fact]
        public void GameState__Menu()
        {
            var result = ToR3EDash(gt with { GameState = GameState.Menu });

            Assert.Equal(1, result.Path("GameInMenus").GetInt32());
            Assert.Equal(0, result.Path("GameInReplay").GetInt32());
        }

        [Fact]
        public void GameState__Replay()
        {
            var result = ToR3EDash(gt with { GameState = GameState.Replay });

            Assert.Equal(0, result.Path("GameInMenus").GetInt32());
            Assert.Equal(1, result.Path("GameInReplay").GetInt32());
        }

        [Fact]
        public void GameState__Driving()
        {
            var result = ToR3EDash(gt with { GameState = GameState.Driving });

            Assert.Equal(0, result.Path("GameInMenus").GetInt32());
            Assert.Equal(0, result.Path("GameInReplay").GetInt32());
        }

        [Fact]
        public void UsingVr__True()
        {
            var result = ToR3EDash(gt with { UsingVR = true });

            Assert.Equal(1, result.Path("GameUsingVr").GetInt32());
        }

        [Fact]
        public void UsingVr__False()
        {
            var result = ToR3EDash(gt with { UsingVR = false });

            Assert.Equal(0, result.Path("GameUsingVr").GetInt32());
        }

        [Fact]
        public void Event__Null()
        {
            var result = ToR3EDash(gt with { Event = null });

            Assert.Equal(-1.0, result.Path("LayoutLength").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
            Assert.Equal(-1.0, result.Path("FuelUseActive").GetDouble());
        }

        [Fact]
        public void Event_Track_SectorsEnd__Empty()
        {
            var result = ToR3EDash(gt.WithSectorsEnd(new IFraction<IDistance>[0]));

            Assert.Equal(-1.0, result.Path("LayoutLength").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
        }

        [Fact]
        public void Event_Track_SectorsEnd__NotAllSectors()
        {
            var result = ToR3EDash(gt.WithSectorsEnd(IFraction.Of(IDistance.FromM(100), 0.10, 0.20)));

            Assert.Equal(100, result.Path("LayoutLength").GetDouble());
            Assert.Equal(0.10, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(0.20, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
        }

        [Fact]
        public void Event_Track_SectorsEnd__AllSectors()
        {
            var result = ToR3EDash(gt.WithSectorsEnd(IFraction.Of(IDistance.FromM(100), 0.10, 0.20, 0.30)));

            Assert.Equal(100, result.Path("LayoutLength").GetDouble());
            Assert.Equal(0.10, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(0.20, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(0.30, result.Path("SectorStartFactors", "Sector3").GetDouble());
        }

        [Fact]
        public void Event_FuelRate()
        {
            var result = ToR3EDash(gt.WithEvent(e => e with { FuelRate = 4.2 }));

            Assert.Equal(4, result.Path("FuelUseActive").GetInt32());
        }

        [Fact]
        public void Session__Null()
        {
            var result = ToR3EDash(gt with { Session = null });

            Assert.Equal(-1, result.Path("SessionType").GetInt32());
            Assert.Equal(-1, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(-1.0, result.Path("SessionPitSpeedLimit").GetDouble());
            Assert.Equal(-1, result.Path("SessionPhase").GetInt32());
            Assert.Equal(-1, result.Path("StartLights").GetInt32());
            Assert.Equal(-1, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(-1.0, result.Path("SessionTimeDuration").GetDouble());
            Assert.Equal(-1, result.Path("PitWindowStatus").GetInt32());
            Assert.Equal(-1, result.Path("PitWindowStart").GetInt32());
            Assert.Equal(-1, result.Path("PitWindowEnd").GetInt32());
        }

        [Theory]
        [InlineData(SessionType.Practice, 0)]
        [InlineData(SessionType.Test, -1)]
        [InlineData(SessionType.Qualify, 1)]
        [InlineData(SessionType.Warmup, 3)]
        [InlineData(SessionType.Race, 2)]
        [InlineData(SessionType.Hotlap, -1)]
        [InlineData(SessionType.TimeAttack, -1)]
        [InlineData(SessionType.Drift, -1)]
        [InlineData(SessionType.Drag, -1)]
        [InlineData(SessionType.HotStint, -1)]
        [InlineData(SessionType.HotStintSuperPole, -1)]
        public void Session_Type(SessionType sessionType, Int32 code)
        {
            var result = ToR3EDash(gt.WithSession(s => s with { Type = sessionType }));

            Assert.Equal(code, result.Path("SessionType").GetInt32());
        }

        [Fact]
        public void Session_PitSpeedLimit()
        {
            var result = ToR3EDash(gt.WithSession(s => s with { PitSpeedLimit = ISpeed.FromMPS(1.0) }));

            Assert.Equal(1.0, result.Path("SessionPitSpeedLimit").GetInt32());
        }

        [Theory]
        [InlineData(SessionPhase.Garage, 1)]
        [InlineData(SessionPhase.Gridwalk, 2)]
        [InlineData(SessionPhase.Formation, 3)]
        [InlineData(SessionPhase.Countdown, 4)]
        [InlineData(SessionPhase.Started, 5)]
        [InlineData(SessionPhase.FullCourseYellow, -1)]
        [InlineData(SessionPhase.Stopped, -1)]
        [InlineData(SessionPhase.Over, 6)]
        public void Session_Phase(SessionPhase sessionPhase, Int32 code)
        {
            var result = ToR3EDash(gt.WithSession(s => s with { Phase = sessionPhase }));

            Assert.Equal(code, result.Path("SessionPhase").GetInt32());
        }

        [Theory]
        [InlineData(LightColour.Green, 0, 5, 6)]
        [InlineData(LightColour.Green, 3, 3, 6)]
        [InlineData(LightColour.Red, 0, 5, 0)]
        [InlineData(LightColour.Red, 1, 5, 1)]
        [InlineData(LightColour.Red, 2, 5, 2)]
        [InlineData(LightColour.Red, 3, 5, 3)]
        [InlineData(LightColour.Red, 4, 5, 4)]
        [InlineData(LightColour.Red, 5, 5, 5)]
        [InlineData(LightColour.Red, 1, 3, 1)]
        [InlineData(LightColour.Red, 2, 3, 3)]
        [InlineData(LightColour.Red, 3, 3, 5)]
        public void Session_StartLights(LightColour colour, UInt32 lit, UInt32 max, Int32 expected)
        {
            var startLights = new StartLights(colour, new BoundedValue<UInt32>(lit, max));
            var result = ToR3EDash(gt.WithSession(s => s with { StartLights = startLights }));

            Assert.Equal(expected, result.Path("StartLights").GetInt32());
        }

        [Fact]
        public void Session_Length__Laps()
        {
            var result = ToR3EDash(gt.WithSession(s => s with { Length = new LapsDuration(3, TimeSpan.FromSeconds(4.5)) }));

            Assert.Equal(1, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(3, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(-1.0, result.Path("SessionTimeDuration").GetDouble());
        }

        [Fact]
        public void Session_Length__Time()
        {
            var result = ToR3EDash(gt.WithSession(s => s with { Length = new TimeDuration(TimeSpan.FromSeconds(3.4), 5) }));

            Assert.Equal(0, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(-1, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(3.4, result.Path("SessionTimeDuration").GetDouble());
        }

        [Fact]
        public void Session_Length__TimePlusLaps()
        {
            var result = ToR3EDash(gt.WithSession(s => s with { Length = new TimePlusLapsDuration(TimeSpan.FromSeconds(3.4), 5, 6) }));

            Assert.Equal(2, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(-1, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(3.4, result.Path("SessionTimeDuration").GetDouble());
        }

        [Fact]
        public void Session_Requirements_PitWindow__Null()
        {
            var result = ToR3EDash(gt.WithSession(s => s with { Requirements = s.Requirements with { PitWindow = null } }));

            Assert.Equal(0, result.Path("PitWindowStatus").GetInt32());
            Assert.Equal(-1, result.Path("PitWindowStart").GetInt32());
            Assert.Equal(-1, result.Path("PitWindowEnd").GetInt32());
        }

        [Theory]
        [InlineData(3, 4, 2, 1)]
        [InlineData(3, 4, 3, 2)]
        [InlineData(3, 4, 4, 2)]
        [InlineData(3, 4, 5, 1)]
        public void Session_Requirements_PitWindow__Laps(UInt32 start, UInt32 end, UInt32 completedLaps, Int32 windowStatus)
        {
            var result = ToR3EDash(gt
                    .WithCurrentVehicle(cv => cv with
                    {
                        CompletedLaps = completedLaps,
                        Pit = cv.Pit with
                        {
                            MandatoryStopsDone = 0,
                            InPitStall = false
                        }
                    })
                    .WithSession(s => s with
                    {
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new LapsDuration(start, null),
                                new LapsDuration(end, null)
                            )
                        }
                    })
                );

            Assert.Equal(windowStatus, result.Path("PitWindowStatus").GetInt32());
            Assert.Equal(start, result.Path("PitWindowStart").GetUInt32());
            Assert.Equal(end, result.Path("PitWindowEnd").GetUInt32());
        }

        [Theory]
        [InlineData(3, 4, 2, 1)]
        [InlineData(3, 4, 3, 2)]
        [InlineData(3, 4, 4, 2)]
        [InlineData(3, 4, 5, 1)]
        public void Session_Requirements_PitWindow__Time(UInt32 start, UInt32 end, UInt32 elapsedTime, Int32 windowStatus)
        {
            var result = ToR3EDash(gt
                    .WithCurrentVehicle(cv => cv with
                    {
                        Pit = cv.Pit with
                        {
                            MandatoryStopsDone = 0,
                            InPitStall = false
                        }
                    })
                    .WithSession(s => s with
                    {
                        ElapsedTime = TimeSpan.FromMinutes(elapsedTime),
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new TimeDuration(TimeSpan.FromMinutes(start), null),
                                new TimeDuration(TimeSpan.FromMinutes(end), null)
                            )
                        }
                    })
                );

            Assert.Equal(windowStatus, result.Path("PitWindowStatus").GetInt32());
            Assert.Equal(start, result.Path("PitWindowStart").GetUInt32());
            Assert.Equal(end, result.Path("PitWindowEnd").GetUInt32());
        }


        [Fact]
        public void Session_Requirements_PitWindow__Laps_NoVehicle()
        {
            var result = ToR3EDash(gt
                    .WithSession(s => s with
                    {
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new LapsDuration(2, null),
                                new LapsDuration(3, null)
                            )
                        }
                    })
                    with { CurrentVehicle = null }
                );

            Assert.Equal(-1, result.Path("PitWindowStatus").GetInt32());
        }

        [Theory]
        [InlineData(3, 5, 2, 1)]
        [InlineData(3, 5, 4, 3)]
        [InlineData(3, 5, 6, 1)]
        public void Session_Requirements_PitWindow__InPitStall(UInt32 start, UInt32 end, UInt32 elapsedTime, Int32 windowStatus)
        {
            var result = ToR3EDash(gt
                    .WithCurrentVehicle(cv => cv with
                    {
                        Pit = cv.Pit with
                        {
                            MandatoryStopsDone = 0,
                            InPitStall = true
                        }
                    })
                    .WithSession(s => s with
                    {
                        ElapsedTime = TimeSpan.FromMinutes(elapsedTime),
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new TimeDuration(TimeSpan.FromMinutes(start), null),
                                new TimeDuration(TimeSpan.FromMinutes(end), null)
                            )
                        }
                    })
                );

            Assert.Equal(windowStatus, result.Path("PitWindowStatus").GetInt32());
        }

        [Theory]
        [InlineData(3, 5, 2)]
        [InlineData(3, 5, 4)]
        [InlineData(3, 5, 6)]
        public void Session_Requirements_PitWindow__MandatoryStopsDone(UInt32 start, UInt32 end, UInt32 elapsedTime)
        {
            var result = ToR3EDash(gt
                    .WithCurrentVehicle(cv => cv with
                    {
                        Pit = cv.Pit with { MandatoryStopsDone = 1 }
                    })
                    .WithSession(s => s with
                    {
                        ElapsedTime = TimeSpan.FromMinutes(elapsedTime),
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new TimeDuration(TimeSpan.FromMinutes(start), null),
                                new TimeDuration(TimeSpan.FromMinutes(end), null)
                            )
                        }
                    })
                );

            Assert.Equal(4, result.Path("PitWindowStatus").GetInt32());
        }

        [Fact]
        public void Player__Null()
        {
            var result = ToR3EDash(gt with { Event = null });

            Assert.Equal(0.0, result.Path("Player", "Position", "X").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "Position", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "Position", "Z").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalAcceleration", "X").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalAcceleration", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalAcceleration", "Z").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalGforce", "X").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalGforce", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalGforce", "Z").GetDouble());
        }

        [Fact]
        public void Player_CgLocation()
        {
            var distance = new Vector3<IDistance>
            (
                IDistance.FromM(1.0),
                IDistance.FromM(2.0),
                IDistance.FromM(3.0)
            );
            var result = ToR3EDash(gt.WithPlayer(p => p with { CgLocation = distance }));

            Assert.Equal(1.0, result.Path("Player", "Position", "X").GetDouble());
            Assert.Equal(2.0, result.Path("Player", "Position", "Y").GetDouble());
            Assert.Equal(3.0, result.Path("Player", "Position", "Z").GetDouble());
        }

        [Fact]
        public void Player_LocalAcceleration()
        {
            var acceleration = new Vector3<IAcceleration>
            (
                IAcceleration.FromMPS2(1.0),
                IAcceleration.FromMPS2(2.0),
                IAcceleration.FromMPS2(3.0)
            );
            var result = ToR3EDash(gt.WithPlayer(p => p with { LocalAcceleration = acceleration }));

            Assert.Equal(1.0, result.Path("Player", "LocalAcceleration", "X").GetDouble());
            Assert.Equal(2.0, result.Path("Player", "LocalAcceleration", "Y").GetDouble());
            Assert.Equal(3.0, result.Path("Player", "LocalAcceleration", "Z").GetDouble());
        }

        [Fact]
        public void Player_LocalGforce()
        {
            var acceleration = new Vector3<IAcceleration>
            (
                IAcceleration.FromG(1.0),
                IAcceleration.FromG(2.0),
                IAcceleration.FromG(3.0)
            );
            var result = ToR3EDash(gt.WithPlayer(p => p with { LocalAcceleration = acceleration }));

            Assert.Equal(1.0, result.Path("Player", "LocalGforce", "X").GetDouble());
            Assert.Equal(2.0, result.Path("Player", "LocalGforce", "Y").GetDouble());
            Assert.Equal(3.0, result.Path("Player", "LocalGforce", "Z").GetDouble());
        }

        #region Test setup

        private static JsonDocument ToR3EDash(GameTelemetry gt)
        {
            var bytes = R3EDashTransformer.ToR3EDash(gt);
            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonDocument.Parse(jsonString);
        }

        #endregion
    }
}

static class GameTelemetryExensions
{
    public static GameTelemetry WithEvent(this GameTelemetry gt, Func<Event, Event> f)
    {
        if (gt.Event is null)
            return gt;
        return gt with { Event = f(gt.Event) };
    }

    public static GameTelemetry WithTrack(this GameTelemetry gt, Func<TrackLayout, TrackLayout> f)
    {
        return gt.WithEvent(e => e with { Track = f( e.Track) });
    }

    public static GameTelemetry WithSectorsEnd(this GameTelemetry gt, IFraction<IDistance>[] sectorsEnd)
    {
        return gt.WithTrack(track => track with { SectorsEnd = sectorsEnd });
    }

    public static GameTelemetry WithSession(this GameTelemetry gt, Func<Session, Session> f)
    {
        if (gt.Session is null)
            return gt;
        return gt with { Session = f(gt.Session) };
    }

    public static GameTelemetry WithCurrentVehicle(this GameTelemetry gt, Func<Vehicle, Vehicle> f)
    {
        if (gt.CurrentVehicle is null)
            return gt;
        return gt with { CurrentVehicle = f(gt.CurrentVehicle) };
    }

    public static GameTelemetry WithPlayer(this GameTelemetry gt, Func<Player, Player> f)
    {
        if (gt.Player is null)
            return gt;
        return gt with { Player = f(gt.Player) };
    }
}