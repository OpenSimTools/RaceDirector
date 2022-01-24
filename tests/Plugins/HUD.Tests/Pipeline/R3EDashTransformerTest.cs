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
using System.Linq;
using static RaceDirector.Pipeline.Telemetry.V0.IVehicleFlags;

namespace HUD.Tests.Pipeline
{
    [UnitTest]
    public class R3EDashTransformerTest
    {
        private IAutoFaker faker = AutoFaker.Create();

        // Note: these cannot be created concurrently, so cannot be static.
        private Bogus.Faker<GameTelemetry> gtFaker = new AutoFaker<GameTelemetry>()
            .Configure(b => b
                .WithBinder<MoqBinder>()
                // For some reason AutoBogus/Moq can't generate IDistance or IFraction<IDistance>
                .WithOverride(agoc => IDistance.FromM(agoc.Faker.Random.Int()))
                .WithOverride(agoc => DistanceFraction.Of(agoc.Generate<IDistance>(), agoc.Faker.Random.Double()))
            );

        private Bogus.Faker<Vehicle> vehicleFaker = new AutoFaker<Vehicle>()
            .Configure(b => b
                .WithBinder<MoqBinder>()
                // For some reason AutoBogus/Moq can't generate IDistance or IFraction<IDistance>
                .WithOverride(agoc => IDistance.FromM(agoc.Faker.Random.Int()))
                .WithOverride(agoc => DistanceFraction.Of(agoc.Generate<IDistance>(), agoc.Faker.Random.Double()))
            );

        private Bogus.Faker<Tyre> tyreFaker = new AutoFaker<Tyre>()
            .Configure(b => b.WithBinder<MoqBinder>());

        // Have to create one par test: concurrent access seems to confuse AutoBogus.
        private GameTelemetry NewGt() => gtFaker.Generate();

        [Fact]
        public void VersionInformation()
        {
            var result = ToR3EDash(NewGt());

            Assert.Equal(2, result.Path("VersionMajor").GetInt32());
            Assert.Equal(11, result.Path("VersionMinor").GetInt32());
        }

        [Fact]
        public void GameState__Driving()
        {
            var result = ToR3EDash(NewGt() with { GameState = GameState.Driving });

            Assert.Equal(0, result.Path("GameInMenus").GetInt32());
            Assert.Equal(0, result.Path("GameInReplay").GetInt32());
        }

        [Fact]
        public void GameState__Menu()
        {
            var result = ToR3EDash(NewGt() with { GameState = GameState.Menu });

            Assert.Equal(1, result.Path("GameInMenus").GetInt32());
            Assert.Equal(0, result.Path("GameInReplay").GetInt32());
        }

        [Fact]
        public void GameState__Replay()
        {
            var result = ToR3EDash(NewGt() with { GameState = GameState.Replay });

            Assert.Equal(0, result.Path("GameInMenus").GetInt32());
            Assert.Equal(1, result.Path("GameInReplay").GetInt32());
        }

        [Fact]
        public void UsingVr__True()
        {
            var result = ToR3EDash(NewGt() with { UsingVR = true });

            Assert.Equal(1, result.Path("GameUsingVr").GetInt32());
        }

        [Fact]
        public void UsingVr__False()
        {
            var result = ToR3EDash(NewGt() with { UsingVR = false });

            Assert.Equal(0, result.Path("GameUsingVr").GetInt32());
        }

        #region Event

        [Fact]
        public void Event__Null()
        {
            var result = ToR3EDash(NewGt() with { Event = null });

            Assert.Equal(-1.0, result.Path("LayoutLength").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
            Assert.Equal(-1.0, result.Path("FuelUseActive").GetDouble());
        }

        [Fact]
        public void Event_FuelRate()
        {
            var result = ToR3EDash(NewGt().WithEvent(e => e with { FuelRate = 4.2 }));

            Assert.Equal(4, result.Path("FuelUseActive").GetInt32());
        }

        [Fact]
        public void Event_Track_SectorsEnd__Empty()
        {
            var result = ToR3EDash(NewGt().WithSectorsEnd(new IFraction<IDistance>[0]));

            Assert.Equal(-1.0, result.Path("LayoutLength").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
        }

        [Fact]
        public void Event_Track_SectorsEnd__NotAllSectors()
        {
            var result = ToR3EDash(NewGt().WithSectorsEnd(DistanceFraction.FromTotal(IDistance.FromM(100), 0.10, 0.20)));

            Assert.Equal(100, result.Path("LayoutLength").GetDouble());
            Assert.Equal(0.10, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(0.20, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
        }

        [Fact]
        public void Event_Track_SectorsEnd__AllSectors()
        {
            var result = ToR3EDash(NewGt().WithSectorsEnd(DistanceFraction.FromTotal(IDistance.FromM(100), 0.10, 0.20, 0.30)));

            Assert.Equal(100, result.Path("LayoutLength").GetDouble());
            Assert.Equal(0.10, result.Path("SectorStartFactors", "Sector1").GetDouble());
            Assert.Equal(0.20, result.Path("SectorStartFactors", "Sector2").GetDouble());
            Assert.Equal(0.30, result.Path("SectorStartFactors", "Sector3").GetDouble());
        }

        #endregion

        #region FocusedVehicle

        [Fact]
        public void FocusedVehicle__Null()
        {
            var result = ToR3EDash(NewGt() with { FocusedVehicle = null });

            Assert.Equal(-1, result.Path("InPitLane").GetInt32());
            Assert.Equal(-1, result.Path("PitState").GetInt32());
            Assert.Equal(-1.0, result.Path("PitElapsedTime").GetDouble());
            Assert.Equal(-1.0, result.Path("PitTotalDuration").GetDouble());
            Assert.Equal(-1, result.Path("Flags", "Yellow").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "Blue").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "Black").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "Green").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "Checkered").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "White").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "BlackAndWhite").GetInt32());
            Assert.Equal(-1, result.Path("PositionClass").GetInt32());
            Assert.Equal(-1, result.Path("Penalties", "DriveThrough").GetInt32());
            Assert.Equal(-1, result.Path("Penalties", "StopAndGo").GetInt32());
            Assert.Equal(-1, result.Path("Penalties", "PitStop").GetInt32());
            Assert.Equal(-1, result.Path("Penalties", "TimeDeduction").GetInt32());
            Assert.Equal(-1, result.Path("Penalties", "SlowDown").GetInt32());
            Assert.Equal(-1.0, result.Path("LapTimeBestSelf").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesBestSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesBestSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesBestSelf", "Sector3").GetDouble());
            Assert.Equal(-1.0, result.Path("LapTimeCurrentSelf").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesCurrentSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesCurrentSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesCurrentSelf", "Sector3").GetDouble());
            Assert.Equal(-1, result.Path("VehicleInfo", "SlotId").GetInt32());
            Assert.Equal(-1, result.Path("VehicleInfo", "ClassPerformanceIndex").GetInt32());
            Assert.Equal(3, result.Path("VehicleInfo", "EngineType").GetInt32());
            Assert.Equal("AA==", result.Path("PlayerName").GetString());
            Assert.Equal(-1, result.Path("ControlType").GetInt32());
            Assert.Equal(-1.0, result.Path("CarSpeed").GetDouble());
            Assert.Equal(0.0, result.Path("CarCgLocation", "X").GetDouble());
            Assert.Equal(0.0, result.Path("CarCgLocation", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("CarCgLocation", "Z").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Pitch").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Yaw").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Roll").GetDouble());
            Assert.Equal(-1.0, result.Path("Throttle").GetDouble());
            Assert.Equal(-1.0, result.Path("Brake").GetDouble());
            Assert.Equal(-1.0, result.Path("Clutch").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Location()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Location = new Vector3<IDistance>(
                            IDistance.FromM(1.2),
                            IDistance.FromM(3.4),
                            IDistance.FromM(5.6)
                        )
                    })
                );

            Assert.Equal(1.2, result.Path("CarCgLocation", "X").GetDouble());
            Assert.Equal(3.4, result.Path("CarCgLocation", "Y").GetDouble());
            Assert.Equal(5.6, result.Path("CarCgLocation", "Z").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_ClassPerformanceIndex()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        ClassPerformanceIndex = 42
                    })
                );

            Assert.Equal(42, result.Path("VehicleInfo", "ClassPerformanceIndex").GetInt32());
        }

        [Theory]
        [InlineData(ControlType.LocalPlayer, 0)]
        [InlineData(ControlType.RemotePlayer, 2)]
        [InlineData(ControlType.AI, 1)]
        [InlineData(ControlType.Replay, 3)]
        public void FocusedVehicle_ControlType(ControlType controlType, int controlTypeId)
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        ControlType = controlType
                    })
                );

            Assert.Equal(controlTypeId, result.Path("ControlType").GetInt32());
        }

        [Fact]
        public void FocusedVehicle_CurrentLapTime()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        CurrentLapTime = new LapTime(
                            TimeSpan.FromSeconds(1.2),
                            new Sectors(
                                new TimeSpan[0],
                                new[] {
                                    TimeSpan.FromSeconds(3.4),
                                    TimeSpan.FromSeconds(5.6),
                                    TimeSpan.FromSeconds(7.8)
                                }
                            )
                        )
                    })
                );

            Assert.Equal(1.2, result.Path("LapTimeCurrentSelf").GetDouble());
            Assert.Equal(3.4, result.Path("SectorTimesCurrentSelf", "Sector1").GetDouble());
            Assert.Equal(5.6, result.Path("SectorTimesCurrentSelf", "Sector2").GetDouble());
            Assert.Equal(7.8, result.Path("SectorTimesCurrentSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_CurrentLapTime__Null()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        CurrentLapTime = null
                    })
                );

            Assert.Equal(-1.0, result.Path("LapTimeCurrentSelf").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesCurrentSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesCurrentSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesCurrentSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_CurrentDriver()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        CurrentDriver = new Driver(
                            Name: "Blues"
                        )
                    })
                );

            Assert.Equal("Blues\0", result.Path("PlayerName").GetBase64String());
        }

        [Theory]
        [InlineData(EngineType.Combustion, 0)]
        [InlineData(EngineType.Electric, 1)]
        [InlineData(EngineType.Hybrid, 2)]
        [InlineData(EngineType.Unknown, 3)]
        public void FocusedVehicle_EngineType(EngineType engineType, int engineTypeId)
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        EngineType = engineType
                    })
                );

            Assert.Equal(engineTypeId, result.Path("VehicleInfo", "EngineType").GetInt32());
        }

        [Fact]
        public void FocusedVehicle_Id()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Id = 42
                    })
                );

            Assert.Equal(42, result.Path("VehicleInfo", "SlotId").GetInt32());
        }

        [Fact]
        public void FocusedVehicle_PositionClass()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        PositionClass = 4
                    })
                );

            Assert.Equal(4, result.Path("PositionClass").GetInt32());
        }

        [Fact]
        public void FocusedVehicle_Speed()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Speed = ISpeed.FromMPS(1.2)
                    })
                );

            Assert.Equal(1.2, result.Path("CarSpeed").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Orientation__Null()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Orientation = null
                    })
                );

            Assert.Equal(0.0, result.Path("CarOrientation", "Pitch").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Yaw").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Roll").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Orientation()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Orientation = new Orientation(
                            Yaw: IAngle.FromRad(1.2),
                            Pitch: IAngle.FromRad(3.4),
                            Roll: IAngle.FromRad(5.6)
                        )
                    })
                );

            Assert.Equal(3.4, result.Path("CarOrientation", "Pitch").GetDouble());
            Assert.Equal(1.2, result.Path("CarOrientation", "Yaw").GetDouble());
            Assert.Equal(5.6, result.Path("CarOrientation", "Roll").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_PersonalBestLapTime__Null()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        BestLapTime = null
                    })
                );

            Assert.Equal(-1.0, result.Path("LapTimeBestSelf").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesBestSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesBestSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("SectorTimesBestSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_PersonalBestLapTime()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        BestLapTime = new LapTime(
                            TimeSpan.FromSeconds(1.2),
                            new Sectors(
                                new TimeSpan[0],
                                new[] {
                                    TimeSpan.FromSeconds(3.4),
                                    TimeSpan.FromSeconds(5.6),
                                    TimeSpan.FromSeconds(7.8)
                                }
                            )
                        )
                    })
                );

            Assert.Equal(1.2, result.Path("LapTimeBestSelf").GetDouble());
            Assert.Equal(3.4, result.Path("SectorTimesBestSelf", "Sector1").GetDouble());
            Assert.Equal(5.6, result.Path("SectorTimesBestSelf", "Sector2").GetDouble());
            Assert.Equal(7.8, result.Path("SectorTimesBestSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Pit_PitLaneState__Null()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with { PitLanePhase = null }
                    })
                    .WithPlayer(p => p with { PitStop = 0 })
                );

            Assert.Equal(0, result.Path("InPitLane").GetInt32());
            Assert.Equal(0, result.Path("PitState").GetInt32());
        }

        [Theory]
        [InlineData(PitLanePhase.Entered, 1, 2)]
        [InlineData(PitLanePhase.Stopped, 1, 3)]
        [InlineData(PitLanePhase.Exiting, 1, 4)]
        public void FocusedVehicle_Pit_PitLaneState(PitLanePhase? pitLaneState, int inPitLane, int pitState)
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with { PitLanePhase = pitLaneState }
                    })
                );

            Assert.Equal(inPitLane, result.Path("InPitLane").GetInt32());
            Assert.Equal(pitState, result.Path("PitState").GetInt32());
        }

        [Fact]
        public void FocusedVehicle_Pit_PitLaneTime__Null()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with { PitLaneTime = null }
                    })
                );

            Assert.Equal(-1.0, result.Path("PitTotalDuration").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Pit_PitLaneTime()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with { PitLaneTime = TimeSpan.FromSeconds(1.2) }
                    })
                );

            Assert.Equal(1.2, result.Path("PitTotalDuration").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Pit_PitStallTime__Null()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with { PitStallTime = null }
                    })
                );

            Assert.Equal(-1.0, result.Path("PitElapsedTime").GetDouble());
        }

        [Fact]
        public void FocusedVehicle_Pit_PitStallTime()
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with { PitStallTime = TimeSpan.FromSeconds(1.2) }
                    })
                );

            Assert.Equal(1.2, result.Path("PitElapsedTime").GetDouble());
        }

        [Theory]
        [InlineData(PenaltyType.Unknown,           0, 0, 0, 0, 0)]
        [InlineData(PenaltyType.SlowDown,          0, 0, 0, 0, 1)]
        [InlineData(PenaltyType.TimeDeduction,     0, 0, 0, 1, 0)]
        [InlineData(PenaltyType.DriveThrough,      1, 0, 0, 0, 0)]
        [InlineData(PenaltyType.PitStop,           0, 0, 1, 0, 0)]
        [InlineData(PenaltyType.StopAndGo10,       0, 1, 0, 0, 0)]
        [InlineData(PenaltyType.StopAndGo20,       0, 1, 0, 0, 0)]
        [InlineData(PenaltyType.StopAndGo30,       0, 1, 0, 0, 0)]
        [InlineData(PenaltyType.GivePositionBack,  0, 0, 0, 0, 0)]
        [InlineData(PenaltyType.RemoveBestLaptime, 0, 0, 0, 0, 0)]
        public void FocusedVehicle_Penalties(PenaltyType penaltyType,
            int driveThrough, int stopAndGo, int pitStop, int timeDeduction, int slowDown)
        {
            var result = ToR3EDash(NewGt()
                    .WithFocusedVehicle(v => v with
                    {
                        Penalties = new Penalty[]
                        {
                            new Penalty(penaltyType, PenaltyReason.Unknown)
                        }
                    })
                );

            Assert.Equal(driveThrough, result.Path("Penalties", "DriveThrough").GetInt32());
            Assert.Equal(stopAndGo, result.Path("Penalties", "StopAndGo").GetInt32());
            Assert.Equal(pitStop, result.Path("Penalties", "PitStop").GetInt32());
            Assert.Equal(timeDeduction, result.Path("Penalties", "TimeDeduction").GetInt32());
            Assert.Equal(slowDown, result.Path("Penalties", "SlowDown").GetInt32());
        }

        [Fact]
        public void FocusedVehicle_Inputs__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithFocusedVehicle(v => v with
                {
                    Inputs = null
                })
            );

            Assert.Equal(-1.0, result.Path("Throttle").GetDouble());
            Assert.Equal(-1.0, result.Path("Brake").GetDouble());
            Assert.Equal(-1.0, result.Path("Clutch").GetDouble());
        }

        #region Player

        [Fact]
        public void Player__Null()
        {
            var result = ToR3EDash(NewGt() with { Player = null });

            Assert.Equal(0.0, result.Path("Player", "Position", "X").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "Position", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "Position", "Z").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalAcceleration", "X").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalAcceleration", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalAcceleration", "Z").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalGforce", "X").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalGforce", "Y").GetDouble());
            Assert.Equal(0.0, result.Path("Player", "LocalGforce", "Z").GetDouble());
            Assert.Equal(-1, result.Path("PitAction").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "YellowOvertake").GetInt32());
            Assert.Equal(-1, result.Path("Flags", "YellowPositionsGained").GetInt32());
            Assert.Equal(-1000.0, result.Path("TimeDeltaBestSelf").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeSelf", "Sector3").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeLeaderClass", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeLeaderClass", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeLeaderClass", "Sector3").GetDouble());
            Assert.Equal(-1.0, result.Path("EngineRps").GetDouble());
            Assert.Equal(-1.0, result.Path("MaxEngineRps").GetDouble());
            Assert.Equal(-1.0, result.Path("UpshiftRps").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Pitch").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Yaw").GetDouble());
            Assert.Equal(0.0, result.Path("CarOrientation", "Roll").GetDouble());
            Assert.Equal(-1.0, result.Path("FuelLeft").GetDouble());
            Assert.Equal(-1.0, result.Path("FuelCapacity").GetDouble());
            Assert.Equal(-1.0, result.Path("FuelPerLap").GetDouble());
            Assert.Equal(-1.0, result.Path("ThrottleRaw").GetDouble());
            Assert.Equal(-1.0, result.Path("BrakeRaw").GetDouble());
            Assert.Equal(-1.0, result.Path("ClutchRaw").GetDouble());
            Assert.Equal(0.0, result.Path("SteerInputRaw").GetDouble());
            Assert.Equal(0, result.Path("SteerWheelRangeDegrees").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Abs").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Tc").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Esp").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Countersteer").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Cornering").GetInt32());
            Assert.Equal(-1, result.Path("Drs", "Equipped").GetInt32());
            Assert.Equal(-1, result.Path("Drs", "Available").GetInt32());
            Assert.Equal(-1, result.Path("Drs", "NumActivationsLeft").GetInt32());
            Assert.Equal(-1, result.Path("Drs", "Engaged").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "Available").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "Engaged").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "AmountLeft").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "EngagedTimeLeft").GetDouble());
            Assert.Equal(-1, result.Path("PushToPass", "WaitTimeLeft").GetDouble());
            Assert.Equal(-1, result.Path("DrsNumActivationsTotal").GetInt32());
            Assert.Equal(-1, result.Path("PtPNumActivationsTotal").GetInt32());
            foreach (var tyre in new[] { "FrontLeft", "FrontRight", "RearLeft", "RearRight" })
            {
                Assert.Equal(-1.0, result.Path("TireGrip", tyre).GetDouble());
                Assert.Equal(-1.0, result.Path("TireWear", tyre).GetDouble());
                Assert.Equal(-1.0, result.Path("TireDirt", tyre).GetDouble());
                Assert.Equal(-1.0, result.Path("TireTemp", tyre, "CurrentTemp", "Left").GetDouble());
                Assert.Equal(-1.0, result.Path("TireTemp", tyre, "CurrentTemp", "Center").GetDouble());
                Assert.Equal(-1.0, result.Path("TireTemp", tyre, "CurrentTemp", "Right").GetDouble());
                Assert.Equal(-1.0, result.Path("TireTemp", tyre, "OptimalTemp").GetDouble());
                Assert.Equal(-1.0, result.Path("TireTemp", tyre, "ColdTemp").GetDouble());
                Assert.Equal(-1.0, result.Path("TireTemp", tyre, "HotTemp").GetDouble());
                Assert.Equal(-1.0, result.Path("BrakeTemp", tyre, "CurrentTemp").GetDouble());
                Assert.Equal(-1.0, result.Path("BrakeTemp", tyre, "OptimalTemp").GetDouble());
                Assert.Equal(-1.0, result.Path("BrakeTemp", tyre, "ColdTemp").GetDouble());
                Assert.Equal(-1.0, result.Path("BrakeTemp", tyre, "HotTemp").GetDouble());
            }
            Assert.Equal(-1.0, result.Path("CarDamage", "Engine").GetDouble());
            Assert.Equal(-1.0, result.Path("CarDamage", "Transmission").GetDouble());
            Assert.Equal(-1.0, result.Path("CarDamage", "Aerodynamics").GetDouble());
            Assert.Equal(-1.0, result.Path("CarDamage", "Suspension").GetDouble());
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
            var result = ToR3EDash(NewGt().WithPlayer(p => p with { CgLocation = distance }));

            Assert.Equal(1.0, result.Path("Player", "Position", "X").GetDouble());
            Assert.Equal(2.0, result.Path("Player", "Position", "Y").GetDouble());
            Assert.Equal(3.0, result.Path("Player", "Position", "Z").GetDouble());
        }

        [Fact]
        public void Player_ClassBestSectors__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    ClassBestSectors = null
                })
            );

            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeLeaderClass", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeLeaderClass", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeLeaderClass", "Sector3").GetDouble());
        }

        [Fact]
        public void Player_ClassBestSectors()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    ClassBestSectors = new Sectors(
                        new[] {
                            TimeSpan.FromSeconds(1.2),
                            TimeSpan.FromSeconds(3.4),
                            TimeSpan.FromSeconds(5.6)
                        },
                        new TimeSpan[0]
                    )
                })
            );

            Assert.Equal(1.2, result.Path("BestIndividualSectorTimeLeaderClass", "Sector1").GetDouble());
            Assert.Equal(3.4, result.Path("BestIndividualSectorTimeLeaderClass", "Sector2").GetDouble());
            Assert.Equal(5.6, result.Path("BestIndividualSectorTimeLeaderClass", "Sector3").GetDouble());
        }

        [Fact]
        public void Player_DrivingAids__AllNull()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    DrivingAids = new DrivingAids(
                        null,
                        null,
                        null,
                        null,
                        null
                    )
                })
            );

            Assert.Equal(-1, result.Path("AidSettings", "Abs").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Tc").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Esp").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Countersteer").GetInt32());
            Assert.Equal(-1, result.Path("AidSettings", "Cornering").GetInt32());
        }

        [Fact]
        public void Player_DrivingAids__Inactive()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    DrivingAids = new DrivingAids(
                        new Aid(1, false),
                        new TractionControl(2, false, null),
                        new Aid(3, false),
                        new Aid(4, false),
                        new Aid(5, false)
                    )
                })
            );

            Assert.Equal(1, result.Path("AidSettings", "Abs").GetInt32());
            Assert.Equal(2, result.Path("AidSettings", "Tc").GetInt32());
            Assert.Equal(3, result.Path("AidSettings", "Esp").GetInt32());
            Assert.Equal(4, result.Path("AidSettings", "Countersteer").GetInt32());
            Assert.Equal(5, result.Path("AidSettings", "Cornering").GetInt32());
        }

        [Fact]
        public void Player_DrivingAids__Active()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    DrivingAids = new DrivingAids(
                        new Aid(1, true),
                        new TractionControl(2, true, null),
                        new Aid(3, true),
                        new Aid(4, true),
                        new Aid(5, true)
                    )
                })
            );

            Assert.Equal(5, result.Path("AidSettings", "Abs").GetInt32());
            Assert.Equal(5, result.Path("AidSettings", "Tc").GetInt32());
            Assert.Equal(5, result.Path("AidSettings", "Esp").GetInt32());
            Assert.Equal(5, result.Path("AidSettings", "Countersteer").GetInt32());
            Assert.Equal(5, result.Path("AidSettings", "Cornering").GetInt32());
        }

        [Fact]
        public void Player_Drs__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Drs = null
                })
            );

            Assert.Equal(0, result.Path("Drs", "Equipped").GetInt32());
            Assert.Equal(0, result.Path("Drs", "Available").GetInt32());
            Assert.Equal(0, result.Path("Drs", "NumActivationsLeft").GetInt32());
            Assert.Equal(0, result.Path("Drs", "Engaged").GetInt32());
            Assert.Equal(-1, result.Path("DrsNumActivationsTotal").GetInt32());
        }

        [Fact]
        public void Player_Drs__NoneActive()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Drs = new ActivationToggled(
                        false,
                        false,
                        null
                    )
                })
            );

            Assert.Equal(1, result.Path("Drs", "Equipped").GetInt32());
            Assert.Equal(0, result.Path("Drs", "Available").GetInt32());
            Assert.Equal(-1, result.Path("Drs", "NumActivationsLeft").GetInt32());
            Assert.Equal(0, result.Path("Drs", "Engaged").GetInt32());
            Assert.Equal(-1, result.Path("DrsNumActivationsTotal").GetInt32());
        }

        [Fact]
        public void Player_Drs__AllActive()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Drs = new ActivationToggled(
                        true,
                        true,
                        new BoundedValue<uint>(2, 3)
                    )
                })
            );

            Assert.Equal(1, result.Path("Drs", "Equipped").GetInt32());
            Assert.Equal(1, result.Path("Drs", "Available").GetInt32());
            Assert.Equal(2, result.Path("Drs", "NumActivationsLeft").GetInt32());
            Assert.Equal(1, result.Path("Drs", "Engaged").GetInt32());
            Assert.Equal(3, result.Path("DrsNumActivationsTotal").GetInt32());
        }

        [Fact]
        public void Player_Engine()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Engine = new Engine(
                        Speed: IAngularSpeed.FromRadPS(1.2),
                        UpshiftSpeed: IAngularSpeed.FromRadPS(3.4),
                        MaxSpeed: IAngularSpeed.FromRadPS(5.6)
                    )
                })
            );

            Assert.Equal(1.2, result.Path("EngineRps").GetDouble());
            Assert.Equal(5.6, result.Path("MaxEngineRps").GetDouble());
            Assert.Equal(3.4, result.Path("UpshiftRps").GetDouble());
        }

        [Fact]
        public void Player_Fuel()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Fuel = new Fuel(
                        Max: 5.6,
                        Left: 3.4,
                        PerLap: 1.2
                    )
                })
            );

            Assert.Equal(3.4, result.Path("FuelLeft").GetDouble());
            Assert.Equal(5.6, result.Path("FuelCapacity").GetDouble());
            Assert.Equal(1.2, result.Path("FuelPerLap").GetDouble());
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
            var result = ToR3EDash(NewGt().WithPlayer(p => p with { LocalAcceleration = acceleration }));

            Assert.Equal(1.0, result.Path("Player", "LocalAcceleration", "X").GetDouble());
            Assert.Equal(2.0, result.Path("Player", "LocalAcceleration", "Y").GetDouble());
            Assert.Equal(3.0, result.Path("Player", "LocalAcceleration", "Z").GetDouble());
        }

        [Fact]
        public void Player_LocalGforce()
        {
            var acceleration = new Vector3<IAcceleration>
            (
                IAcceleration.FromApproxG(1.0),
                IAcceleration.FromApproxG(2.0),
                IAcceleration.FromApproxG(3.0)
            );
            var result = ToR3EDash(NewGt().WithPlayer(p => p with { LocalAcceleration = acceleration }));

            Assert.Equal(1.0, result.Path("Player", "LocalGforce", "X").GetDouble());
            Assert.Equal(2.0, result.Path("Player", "LocalGforce", "Y").GetDouble());
            Assert.Equal(3.0, result.Path("Player", "LocalGforce", "Z").GetDouble());
        }

        [Fact]
        public void Player_PersonalBestDelta__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    PersonalBestDelta = null
                })
            );

            Assert.Equal(-1000.0, result.Path("TimeDeltaBestSelf").GetDouble());
        }

        [Fact]
        public void Player_PersonalBestDelta()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    PersonalBestDelta = TimeSpan.FromSeconds(-1.2)
                })
            );

            Assert.Equal(-1.2, result.Path("TimeDeltaBestSelf").GetDouble());
        }

        [Fact]
        public void Player_PersonalBestSectors__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    PersonalBestSectors = null
                })
            );

            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, result.Path("BestIndividualSectorTimeSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void Player_PersonalBestSectors()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    PersonalBestSectors = new Sectors(
                        new[] {
                            TimeSpan.FromSeconds(1.2),
                            TimeSpan.FromSeconds(3.4),
                            TimeSpan.FromSeconds(5.6)
                        },
                        new TimeSpan[0]
                    )
                })
            );

            Assert.Equal(1.2, result.Path("BestIndividualSectorTimeSelf", "Sector1").GetDouble());
            Assert.Equal(3.4, result.Path("BestIndividualSectorTimeSelf", "Sector2").GetDouble());
            Assert.Equal(5.6, result.Path("BestIndividualSectorTimeSelf", "Sector3").GetDouble());
        }

        [Theory]
        [InlineData(PlayerPitStop.None, 0, 0)]
        [InlineData(PlayerPitStop.Requested, 1, 0)]
        [InlineData(PlayerPitStop.Preparing, 0, 1)]
        [InlineData(PlayerPitStop.ServingPenalty, 0, 2)]
        [InlineData(PlayerPitStop.DriverChange, 0, 4)]
        [InlineData(PlayerPitStop.Refuelling, 0, 8)]
        [InlineData(PlayerPitStop.ChangeFrontTyres, 0, 16)]
        [InlineData(PlayerPitStop.ChangeRearTyres, 0, 32)]
        [InlineData(PlayerPitStop.RepairBody, 0, 64)]
        [InlineData(PlayerPitStop.RepairFrontWing, 0, 128)]
        [InlineData(PlayerPitStop.RepairRearWing, 0, 256)]
        [InlineData(PlayerPitStop.RepairSuspension, 0, 512)]
        [InlineData(PlayerPitStop.Requested | PlayerPitStop.Preparing | PlayerPitStop.ServingPenalty, 1, 3)]
        public void Player_PitStop(PlayerPitStop pitStop, int pitState, int pitAction)
        {
            var result = ToR3EDash(NewGt()
                .WithFocusedVehicle(v => v with { Pit = v.Pit with { PitLanePhase = null } })
                .WithPlayer(p => p with { PitStop = pitStop }));

            Assert.Equal(pitState, result.Path("PitState").GetInt32());
            Assert.Equal(pitAction, result.Path("PitAction").GetInt32());
        }

        [Fact]
        public void Player_PushToPass__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    PushToPass = null
                })
            );

            Assert.Equal(-1, result.Path("PushToPass", "Available").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "Engaged").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "AmountLeft").GetInt32());
            Assert.Equal(-1, result.Path("PushToPass", "EngagedTimeLeft").GetDouble());
            Assert.Equal(-1, result.Path("PushToPass", "WaitTimeLeft").GetDouble());
            Assert.Equal(-1, result.Path("PtPNumActivationsTotal").GetInt32());
        }

        [Fact]
        public void Player_PushToPass()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    PushToPass = new WaitTimeToggled(
                        true,
                        true,
                        new BoundedValue<uint>(2, 3),
                        TimeSpan.FromSeconds(4.5),
                        TimeSpan.FromSeconds(6.7)
                        )
                })
            );

            Assert.Equal(1, result.Path("PushToPass", "Available").GetInt32());
            Assert.Equal(1, result.Path("PushToPass", "Engaged").GetInt32());
            Assert.Equal(2, result.Path("PushToPass", "AmountLeft").GetInt32());
            Assert.Equal(4.5, result.Path("PushToPass", "EngagedTimeLeft").GetDouble());
            Assert.Equal(6.7, result.Path("PushToPass", "WaitTimeLeft").GetDouble());
            Assert.Equal(3, result.Path("PtPNumActivationsTotal").GetInt32());
        }

        [Fact]
        public void Player_RawInputs()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    RawInputs = new RawInputs(
                        Steering: 0.1,
                        Throttle: 0.2,
                        Brake: 0.3,
                        Clutch: 0.4,
                        SteerWheelRange: IAngle.FromDeg(56.7)
                    )
                })
            );

            Assert.Equal(0.2, result.Path("ThrottleRaw").GetDouble());
            Assert.Equal(0.3, result.Path("BrakeRaw").GetDouble());
            Assert.Equal(0.4, result.Path("ClutchRaw").GetDouble());
            Assert.Equal(0.1, result.Path("SteerInputRaw").GetDouble());
            Assert.Equal(57, result.Path("SteerWheelRangeDegrees").GetInt32());
        }

        [Fact]
        public void Player_Tyres()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Tyres = new[]
                    {
                        new []
                        {
                            tyreFaker.Generate() with {
                                Dirt = 1.1,
                                Grip = 1.2,
                                Wear = 1.3,
                                Temperatures = new TemperaturesMatrix(
                                    CurrentTemperatures: new []
                                    {
                                        new [] {
                                            ITemperature.FromC(3.1),
                                            ITemperature.FromC(3.2),
                                            ITemperature.FromC(3.3)
                                        }
                                    },
                                    OptimalTemperature: ITemperature.FromC(2.1),
                                    ColdTemperature: ITemperature.FromC(2.2),
                                    HotTemperature: ITemperature.FromC(2.3)
                                ),
                                BrakeTemperatures = new TemperaturesSingle(
                                    CurrentTemperature: ITemperature.FromC(4.1),
                                    OptimalTemperature: ITemperature.FromC(4.2),
                                    ColdTemperature: ITemperature.FromC(4.3),
                                    HotTemperature: ITemperature.FromC(4.4)
                                )
                            }
                        }
                    }
                })
            );

            Assert.Equal(1.2, result.Path("TireGrip", "FrontLeft").GetDouble());
            Assert.Equal(1.3, result.Path("TireWear", "FrontLeft").GetDouble());
            Assert.Equal(1.1, result.Path("TireDirt", "FrontLeft").GetDouble());
            Assert.Equal(3.1, result.Path("TireTemp", "FrontLeft", "CurrentTemp", "Left").GetDouble());
            Assert.Equal(3.2, result.Path("TireTemp", "FrontLeft", "CurrentTemp", "Center").GetDouble());
            Assert.Equal(3.3, result.Path("TireTemp", "FrontLeft", "CurrentTemp", "Right").GetDouble());
            Assert.Equal(2.1, result.Path("TireTemp", "FrontLeft", "OptimalTemp").GetDouble());
            Assert.Equal(2.2, result.Path("TireTemp", "FrontLeft", "ColdTemp").GetDouble());
            Assert.Equal(2.3, result.Path("TireTemp", "FrontLeft", "HotTemp").GetDouble());
            Assert.Equal(4.1, result.Path("BrakeTemp", "FrontLeft", "CurrentTemp").GetDouble());
            Assert.Equal(4.2, result.Path("BrakeTemp", "FrontLeft", "OptimalTemp").GetDouble());
            Assert.Equal(4.3, result.Path("BrakeTemp", "FrontLeft", "ColdTemp").GetDouble());
            Assert.Equal(4.4, result.Path("BrakeTemp", "FrontLeft", "HotTemp").GetDouble());
        }

        [Theory]
        [InlineData(0, 3, false, false, true, true)]
        [InlineData(1, 2, true, false, true, true)]
        [InlineData(2, 1, true, true, true, false)]
        [InlineData(3, 0, true, true, false, false)]
        public void Player_Tyres__Present(int tyresFront, int tyresRear, bool frontLeftPresent,
            bool frontRightPresent, bool rearLeftPresent, bool rearRightPresent)
        {
            var grip = 2.2;
            Func<int, Tyre[]> tyresWithGrip = (int n) => Enumerable.Range(0, n).Select(_ =>
                tyreFaker.Generate() with { Grip = grip }
            ).ToArray();

            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Tyres = new[] {
                        tyresWithGrip(tyresFront),
                        tyresWithGrip(tyresRear)
                    }
                })
            );

            Assert.Equal(frontLeftPresent ? grip : -1.0, result.Path("TireGrip", "FrontLeft").GetDouble());
            Assert.Equal(frontRightPresent ? grip : -1.0, result.Path("TireGrip", "FrontRight").GetDouble());
            Assert.Equal(rearLeftPresent ? grip : -1.0, result.Path("TireGrip", "RearLeft").GetDouble());
            Assert.Equal(rearRightPresent ? grip : -1.0, result.Path("TireGrip", "RearRight").GetDouble());
        }

        [Theory]
        [InlineData(0, false, false, false)]
        [InlineData(1, true, false, false)]
        [InlineData(2, true, true, false)]
        [InlineData(3, true, true, true)]
        [InlineData(4, true, true, true)]
        public void Player_Tyres_Temperatures_CurrentTemperatures__Present(int temperatures, bool leftPresent,
            bool centrePresent, bool rightPresent)
        {
            var temp = 2.2;
            Func<int, Tyre> tyreWithCurrentTemperatures = (int n) =>
            {
                var t = tyreFaker.Generate();
                var currentTemperatures = Enumerable.Range(0, n).Select(_ =>
                    ITemperature.FromC(temp)
                ).ToArray();
                return t with
                {
                    Temperatures = t.Temperatures with
                    {
                        CurrentTemperatures = new[] {
                            currentTemperatures
                        }
                    }
                };
            };

            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    Tyres = new[] {
                        new [] {
                            tyreWithCurrentTemperatures(temperatures)
                        }
                    }
                })
            );

            Assert.Equal(leftPresent ? temp : -1.0, result.Path("TireTemp", "FrontLeft", "CurrentTemp", "Left").GetDouble());
            Assert.Equal(centrePresent ? temp : -1.0, result.Path("TireTemp", "FrontLeft", "CurrentTemp", "Center").GetDouble());
            Assert.Equal(rightPresent ? temp : -1.0, result.Path("TireTemp", "FrontLeft", "CurrentTemp", "Right").GetDouble());
        }

        [Fact]
        public void Player_VehicleDamage()
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with
                {
                    VehicleDamage = new VehicleDamage(
                        AerodynamicsPercent: 0.1,
                        EnginePercent: 0.2,
                        SuspensionPercent: 0.3,
                        TransmissionPercent: 0.4
                    )
                })
            );

            Assert.Equal(0.2, result.Path("CarDamage", "Engine").GetDouble());
            Assert.Equal(0.4, result.Path("CarDamage", "Transmission").GetDouble());
            Assert.Equal(0.1, result.Path("CarDamage", "Aerodynamics").GetDouble());
            Assert.Equal(0.3, result.Path("CarDamage", "Suspension").GetDouble());
        }

        [Theory]
        [InlineData(SessionPhase.Garage, -1)]
        [InlineData(SessionPhase.Gridwalk, -1)]
        [InlineData(SessionPhase.Formation, -1)]
        [InlineData(SessionPhase.Countdown, -1)]
        [InlineData(SessionPhase.Started, 3)]
        [InlineData(SessionPhase.FullCourseYellow, 3)]
        [InlineData(SessionPhase.Stopped, 3)]
        [InlineData(SessionPhase.Over, 3)]
        public void Player_Warnings_GiveBackPositions(SessionPhase sessionPhase, int giveBackPositions)
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with { Phase = sessionPhase })
                .WithPlayerWarnings(w => w with
                {
                    GiveBackPositions = 3
                })
            );

            Assert.Equal(giveBackPositions, result.Path("Flags", "YellowPositionsGained").GetInt32());
        }

        [Theory]
        [InlineData(false, 0)]
        [InlineData(true, 1)]
        public void Player_OvertakeAllowed(bool overtakeAllowed, int yellowOvertake)
        {
            var result = ToR3EDash(NewGt()
                .WithPlayer(p => p with {
                    OvertakeAllowed = overtakeAllowed
                })
            );
            Assert.Equal(yellowOvertake, result.Path("Flags", "YellowOvertake").GetInt32());
        }

        #endregion

        #endregion

        #region Session

        [Fact]
        public void Session__Null()
        {
            var result = ToR3EDash(NewGt() with { Session = null });

            Assert.Equal(-1, result.Path("GameInMenus").GetInt32());
            Assert.Equal(-1, result.Path("GameInReplay").GetInt32());
            Assert.Equal(-1, result.Path("GameUsingVr").GetInt32());

            Assert.Equal(-1, result.Path("SessionType").GetInt32());
            Assert.Equal(-1, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(-1.0, result.Path("SessionPitSpeedLimit").GetDouble());
            Assert.Equal(-1, result.Path("SessionPhase").GetInt32());
            Assert.Equal(-1, result.Path("StartLights").GetInt32());
            Assert.Equal(-1, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(-1.0, result.Path("SessionTimeDuration").GetDouble());
            Assert.Equal(-1, result.Path("PitWindowStart").GetInt32());
            Assert.Equal(-1, result.Path("PitWindowEnd").GetInt32());
        }

        [Fact]
        public void Session_Length__Laps()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Length = new LapsDuration(3, TimeSpan.FromSeconds(4.5)),
                TimeRemaining = null,
                WaitTime = null,
            }));

            Assert.Equal(1, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(3, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(-1.0, result.Path("SessionTimeDuration").GetDouble());
            Assert.Equal(-1.0, result.Path("SessionTimeRemaining").GetDouble());
        }

        [Fact]
        public void Session_Length__Laps__Finished()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Length = new LapsDuration(1, TimeSpan.FromSeconds(2.3)),
                WaitTime = TimeSpan.FromSeconds(4.5),
            }));

            Assert.Equal(4.5, result.Path("SessionTimeRemaining").GetDouble());
        }

        [Fact]
        public void Session_Length__Time()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Length = new TimeDuration(TimeSpan.FromSeconds(1.2), 5),
                TimeRemaining = TimeSpan.FromSeconds(3.4),
                WaitTime = null,
            }));

            Assert.Equal(0, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(-1, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(1.2, result.Path("SessionTimeDuration").GetDouble());
            Assert.Equal(3.4, result.Path("SessionTimeRemaining").GetDouble());
        }

        [Fact]
        public void Session_Length__Time__Finished()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Length = new TimeDuration(TimeSpan.FromSeconds(1.2), 3),
                WaitTime = TimeSpan.FromSeconds(4.5),
            }));

            Assert.Equal(4.5, result.Path("SessionTimeRemaining").GetDouble());
        }

        [Fact]
        public void Session_Length__TimePlusLaps()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Length = new TimePlusLapsDuration(TimeSpan.FromSeconds(1.2), 3, 4),
                TimeRemaining = TimeSpan.FromSeconds(5.6),
                WaitTime = null,
            }));

            Assert.Equal(2, result.Path("SessionLengthFormat").GetInt32());
            Assert.Equal(-1, result.Path("NumberOfLaps").GetInt32());
            Assert.Equal(1.2, result.Path("SessionTimeDuration").GetDouble());
            Assert.Equal(5.6, result.Path("SessionTimeRemaining").GetDouble());
        }

        [Fact]
        public void Session_Length__TimePlusLaps__Finished()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Length = new TimePlusLapsDuration(TimeSpan.FromSeconds(1.2), 3, 4),
                WaitTime = TimeSpan.FromSeconds(5.6),
            }));

            Assert.Equal(5.6, result.Path("SessionTimeRemaining").GetDouble());
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
        public void Session_Phase(SessionPhase sessionPhase, int code)
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with { Phase = sessionPhase }));

            Assert.Equal(code, result.Path("SessionPhase").GetInt32());
        }

        [Fact]
        public void Session_PitSpeedLimit()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with { PitSpeedLimit = ISpeed.FromMPS(0.1) }));

            Assert.Equal(0.1, result.Path("SessionPitSpeedLimit").GetDouble());
        }

        [Theory]
        [InlineData(3, 4, 2, 3, 4)]
        [InlineData(3, 4, 3, 3, 4)]
        [InlineData(3, 4, 4, -1, -1)]
        [InlineData(3, 4, 5, -1, -1)]
        public void Session_Requirements_PitWindow__Laps(
            uint start, uint finish, uint completedLaps, int pitWindowStart, int pitWindowEnd)
        {
            var result = ToR3EDash(NewGt()
                    .WithSession(s => s with
                    {
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new LapsDuration(start, null),
                                new LapsDuration(finish, null)
                            )
                        }
                    })
                    .WithFocusedVehicle(v => v with
                    {
                        CompletedLaps = completedLaps
                    })
                );

            Assert.Equal(pitWindowStart, result.Path("PitWindowStart").GetInt32());
            Assert.Equal(pitWindowEnd, result.Path("PitWindowEnd").GetInt32());
        }

        [Theory]
        [InlineData(3, 4, 2, 3, 4)]
        [InlineData(3, 4, 3, 3, 4)]
        [InlineData(3, 4, 4, -1, -1)]
        [InlineData(3, 4, 5, -1, -1)]
        public void Session_Requirements_PitWindow__Time(
            uint start, uint finish, uint elapsedTime, int pitWindowStart, int pitWindowEnd)
        {
            var result = ToR3EDash(NewGt()
                    .WithSession(s => s with
                    {
                        ElapsedTime = TimeSpan.FromMinutes(elapsedTime),
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new TimeDuration(TimeSpan.FromMinutes(start), null),
                                new TimeDuration(TimeSpan.FromMinutes(finish), null)
                            )
                        }
                    })
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with
                        {
                            MandatoryStopsDone = 0,
                            PitLanePhase = null
                        }
                    })
                );

            Assert.Equal(pitWindowStart, result.Path("PitWindowStart").GetInt32());
            Assert.Equal(pitWindowEnd, result.Path("PitWindowEnd").GetInt32());
        }

        [Fact]
        public void Session_Requirements_PitWindow__Null()
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with
            {
                Requirements = s.Requirements with { PitWindow = null }
            }));

            Assert.Equal(-1, result.Path("PitWindowStart").GetInt32());
            Assert.Equal(-1, result.Path("PitWindowEnd").GetInt32());
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
        public void Session_StartLights(LightColour colour, uint lit, uint max, int expected)
        {
            var startLights = new StartLights(colour, new BoundedValue<uint>(lit, max));
            var result = ToR3EDash(NewGt().WithSession(s => s with { StartLights = startLights }));

            Assert.Equal(expected, result.Path("StartLights").GetInt32());
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
        public void Session_Type(SessionType sessionType, int code)
        {
            var result = ToR3EDash(NewGt().WithSession(s => s with { Type = sessionType }));

            Assert.Equal(code, result.Path("SessionType").GetInt32());
        }

        #endregion

        #region Vehicles

        [Fact]
        public void Vehicles()
        {
            var result = ToR3EDash(NewGt() with {
                Vehicles = new[] {
                    vehicleFaker.Generate() with
                    {
                        Id = 2,
                        ClassPerformanceIndex = 3,
                        PositionClass = 4,
                        CompletedLaps = 7,
                        CurrentLapDistance = DistanceFraction.FromTotal(IDistance.FromM(0.11), 1.0),
                        Location = new Vector3<IDistance>(
                            X: IDistance.FromM(0.12),
                            Y: IDistance.FromM(0.13),
                            Z: IDistance.FromM(0.14)
                        ),
                        CurrentDriver = new Driver(
                            Name: "Blues"
                        )
                    }
                }
            });

            var driverData = result.Path("DriverData").EnumerateArray().Single();

            Assert.Equal("Blues\0", driverData.Path("DriverInfo", "Name").GetBase64String());
            Assert.Equal(2, driverData.Path("DriverInfo", "SlotId").GetInt32());
            Assert.Equal(3, driverData.Path("DriverInfo", "ClassPerformanceIndex").GetInt32());
            Assert.Equal(4, driverData.Path("PlaceClass").GetInt32());
            Assert.Equal(0.11, driverData.Path("LapDistance").GetDouble());
            Assert.Equal(0.12, driverData.Path("Position", "X").GetDouble());
            Assert.Equal(0.13, driverData.Path("Position", "Y").GetDouble());
            Assert.Equal(0.14, driverData.Path("Position", "Z").GetDouble());
            Assert.Equal(7, driverData.Path("CompletedLaps").GetInt32());
        }

        [Fact]
        public void Vehicles_BestLapTime__Null()
        {
            var result = ToR3EDash(NewGt() with
            {
                Vehicles = new[] {
                    vehicleFaker.Generate() with
                    {
                        BestLapTime = null
                    }
                }
            });

            var driverData = result.Path("DriverData").EnumerateArray().Single();

            Assert.Equal(-1.0, driverData.Path("SectorTimeBestSelf", "Sector1").GetDouble());
            Assert.Equal(-1.0, driverData.Path("SectorTimeBestSelf", "Sector2").GetDouble());
            Assert.Equal(-1.0, driverData.Path("SectorTimeBestSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void Vehicles_BestLapTime()
        {
            var result = ToR3EDash(NewGt() with
            {
                Vehicles = new[] {
                    vehicleFaker.Generate() with
                    {
                        BestLapTime = new LapTime(
                            Overall: TimeSpan.FromSeconds(0), // <==
                            Sectors: new Sectors(
                                Individual: new TimeSpan[0], // <==
                                Cumulative: new []
                                {
                                    TimeSpan.FromSeconds(0.08),
                                    TimeSpan.FromSeconds(0.09),
                                    TimeSpan.FromSeconds(0.10)
                                }
                            )
                        )
                    }
                }
            });

            var driverData = result.Path("DriverData").EnumerateArray().Single();

            Assert.Equal(0.08, driverData.Path("SectorTimeBestSelf", "Sector1").GetDouble());
            Assert.Equal(0.09, driverData.Path("SectorTimeBestSelf", "Sector2").GetDouble());
            Assert.Equal(0.10, driverData.Path("SectorTimeBestSelf", "Sector3").GetDouble());
        }

        [Fact]
        public void Vehicles_Gaps__Null()
        {
            var result = ToR3EDash(NewGt() with
            {
                Vehicles = new[] {
                    vehicleFaker.Generate() with
                    {
                        GapAhead = null,
                        GapBehind = null
                    }
                }
            });

            var driverData = result.Path("DriverData").EnumerateArray().Single();

            Assert.Equal(-1.0, driverData.Path("TimeDeltaFront").GetDouble());
            Assert.Equal(-1.0, driverData.Path("TimeDeltaBehind").GetDouble());
        }

        [Fact]
        public void Vehicles_Gaps()
        {
            var result = ToR3EDash(NewGt() with
            {
                Vehicles = new[] {
                    vehicleFaker.Generate() with
                    {
                        GapAhead = TimeSpan.FromSeconds(0.05),
                        GapBehind = TimeSpan.FromSeconds(0.06)
                    }
                }
            });

            var driverData = result.Path("DriverData").EnumerateArray().Single();

            Assert.Equal(0.05, driverData.Path("TimeDeltaFront").GetDouble());
            Assert.Equal(0.06, driverData.Path("TimeDeltaBehind").GetDouble());
        }

        #endregion

        #region Mixed

        // PitWindowStatus

        [Fact]
        public void Out_PitWindowStatus__Session__Null()
        {
            var result = ToR3EDash(NewGt() with { Session = null });

            Assert.Equal(-1, result.Path("PitWindowStatus").GetInt32());
        }

        [Fact]
        public void Out_PitWindowStatus__Session_PitLaneOpen()
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    PitLaneOpen = false
                }));

            Assert.Equal(0, result.Path("PitWindowStatus").GetInt32());
        }

        [Fact]
        public void Out_PitWindowStatus__FocusedVehicle__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    PitLaneOpen = true // Precondition
                })
                .WithFocusedVehicle(_ => null));

            Assert.Equal(-1, result.Path("PitWindowStatus").GetInt32());
        }

        [Fact]
        public void Out_PitWindowStatus__FocusedVehicle_Pit_PitLanePhase()
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    PitLaneOpen = true // Precondition
                })
                .WithFocusedVehicle(v => v with
                {
                    Pit = v.Pit with
                    {
                        PitLanePhase = PitLanePhase.Stopped
                    }
                }));

            Assert.Equal(3, result.Path("PitWindowStatus").GetInt32());
        }

        [Fact]
        public void Out_PitWindowStatus__Session_Requirements_PitWindow__Null()
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    Requirements = s.Requirements with { PitWindow = null },
                    PitLaneOpen = true // Precondition
                })
                .WithFocusedVehicle(v => v with
                {
                    Pit = v.Pit with
                    {
                        PitLanePhase = null, // Precondition
                    }
                }));

            Assert.Equal(2, result.Path("PitWindowStatus").GetInt32());
        }

        [Fact]
        public void Out_PitWindowStatus__FocusedVehicle_Pit_MandatoryStopsDone()
        {
            var mandatoryPitStopsDone = faker.Generate<UInt16>();
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    Requirements = s.Requirements with
                    {
                        MandatoryPitStops = mandatoryPitStopsDone
                    },
                    PitLaneOpen = true // Precondition
                })
                .WithFocusedVehicle(v => v with
                {
                    Pit = v.Pit with
                    {
                        MandatoryStopsDone = mandatoryPitStopsDone,
                        PitLanePhase = null // Precondition
                    }
                }));

            Assert.Equal(4, result.Path("PitWindowStatus").GetInt32());
        }

        [Theory]
        [InlineData(3, 4, 2, 1)]
        [InlineData(3, 4, 3, 2)]
        [InlineData(3, 4, 4, 1)]
        [InlineData(3, 4, 5, 1)]
        public void Out_PitWindowStatus__Session_Requirements_PitWindow__Laps(
            uint start, uint finish, uint completedLaps, int pitWindowStatus)
        {
            var mandatoryPitStopsDone = faker.Generate<UInt16>();
            var result = ToR3EDash(NewGt()
                    .WithSession(s => s with
                    {
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new LapsDuration(start, null),
                                new LapsDuration(finish, null)
                            ),
                            MandatoryPitStops = mandatoryPitStopsDone + 1u // Precondition
                        },
                        PitLaneOpen = true // Precondition
                    })
                    .WithFocusedVehicle(v => v with
                    {
                        CompletedLaps = completedLaps,
                        Pit = v.Pit with {
                            MandatoryStopsDone = mandatoryPitStopsDone, // Precondition
                            PitLanePhase = null // Precondition
                        }
                    })
                );

            Assert.Equal(pitWindowStatus, result.Path("PitWindowStatus").GetInt32());
        }

        [Theory]
        [InlineData(3, 4, 2, 1)]
        [InlineData(3, 4, 3, 2)]
        [InlineData(3, 4, 4, 1)]
        [InlineData(3, 4, 5, 1)]
        public void Out_PitWindowStatus__Session_Requirements_PitWindow__Time(
            uint start, uint finish, uint elapsedTime, int pitWindowStatus)
        {
            var mandatoryPitStopsDone = faker.Generate<UInt16>();
            var result = ToR3EDash(NewGt()
                    .WithSession(s => s with
                    {
                        ElapsedTime = TimeSpan.FromMinutes(elapsedTime),
                        Requirements = s.Requirements with
                        {
                            PitWindow = new Interval<IPitWindowBoundary>(
                                new TimeDuration(TimeSpan.FromMinutes(start), null),
                                new TimeDuration(TimeSpan.FromMinutes(finish), null)
                            ),
                            MandatoryPitStops = mandatoryPitStopsDone + 1u // Precondition
                        },
                        PitLaneOpen = true // Precondition
                    })
                    .WithFocusedVehicle(v => v with
                    {
                        Pit = v.Pit with
                        {
                            MandatoryStopsDone = mandatoryPitStopsDone, // Precondition
                            PitLanePhase = null // Precondition
                        }
                    })
                );

            Assert.Equal(pitWindowStatus, result.Path("PitWindowStatus").GetInt32());
        }

        [Theory]
        [InlineData(SessionPhase.Garage, -1, -1, 0, -1, 0, -1, -1)]
        [InlineData(SessionPhase.Gridwalk, -1, -1, 0, -1, 0, -1, -1)]
        [InlineData(SessionPhase.Formation, -1, -1, 0, -1, 0, -1, -1)]
        [InlineData(SessionPhase.Countdown, -1, -1, 0, -1, 0, -1, -1)]
        [InlineData(SessionPhase.Started, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(SessionPhase.FullCourseYellow, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(SessionPhase.Stopped, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(SessionPhase.Over, -1, -1, 0, -1, 0, -1, -1)]
        public void Out_Flags__Null(SessionPhase sessionPhase,
            int yellow, int blue, int black, int green,
            int chequered, int white, int blackAndWhite)
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    Phase = sessionPhase
                })
                .WithFocusedVehicleFlags(f => f with
                {
                    Green = null,
                    Blue = null,
                    Yellow = null,
                    White = null,
                    Chequered = null,
                    Black = null,
                    BlackWhite = null
                }));

            Assert.Equal(yellow, result.Path("Flags", "Yellow").GetInt32());
            Assert.Equal(blue, result.Path("Flags", "Blue").GetInt32());
            Assert.Equal(black, result.Path("Flags", "Black").GetInt32());
            Assert.Equal(green, result.Path("Flags", "Green").GetInt32());
            Assert.Equal(chequered, result.Path("Flags", "Checkered").GetInt32());
            Assert.Equal(white, result.Path("Flags", "White").GetInt32());
            Assert.Equal(blackAndWhite, result.Path("Flags", "BlackAndWhite").GetInt32());
        }

        [Fact]
        public void Out_Flags()
        {
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with
                {
                    Phase = SessionPhase.Started
                })
                .WithFocusedVehicleFlags(f => f with
                {
                    Green = new GreenFlag(IVehicleFlags.GreenReason.Unknown),
                    Blue = new BlueFlag(IVehicleFlags.BlueReason.Unknown),
                    White = new WhiteFlag(IVehicleFlags.WhiteReason.Unknown),
                    Chequered = new Flag(),
                    Black = new Flag()
                }));

            Assert.Equal(1, result.Path("Flags", "Blue").GetInt32());
            Assert.Equal(1, result.Path("Flags", "Black").GetInt32());
            Assert.Equal(1, result.Path("Flags", "Green").GetInt32());
            Assert.Equal(1, result.Path("Flags", "Checkered").GetInt32());
            Assert.Equal(1, result.Path("Flags", "White").GetInt32());
        }

        [Fact]
        public void Out_Flags_Yellow()
        {
            // Separate for when we are going to implement other fields like CausedIt
            var result = ToR3EDash(NewGt()
                .WithSession(s => s with { Phase = SessionPhase.Started })
                .WithFocusedVehicleFlags(f => f with
                {
                    Yellow = new YellowFlag(IVehicleFlags.YellowReason.Unknown)
                }));

            Assert.Equal(1, result.Path("Flags", "Yellow").GetInt32());
        }

        [Theory]
        [InlineData(BlackWhiteReason.Unknown, 0, 0)]
        [InlineData(BlackWhiteReason.IgnoredBlueFlags, 0, 0)]
        [InlineData(BlackWhiteReason.IgnoredBlueFlags, 1, 1)]
        [InlineData(BlackWhiteReason.IgnoredBlueFlags, 2, 2)]
        [InlineData(BlackWhiteReason.IgnoredBlueFlags, 3, 0)]
        [InlineData(BlackWhiteReason.WrongWay, 0, 3)]
        [InlineData(BlackWhiteReason.Cutting, 0, 4)]
        public void Out_Flags_BlackAndWhite(BlackWhiteReason reason, uint blueFlagWarnings, int value)
        {
            var result = ToR3EDash(NewGt()
                    .WithSession(s => s with { Phase = SessionPhase.Started })
                    .WithFocusedVehicleFlags(f => f with
                    {
                        BlackWhite = new BlackWhiteFlag(reason)
                    })
                    .WithPlayerWarnings(w => w with
                    {
                        BlueFlagWarnings = new BoundedValue<uint>(blueFlagWarnings, 2)
                    })
                );

            Assert.Equal(value, result.Path("Flags", "BlackAndWhite").GetInt32());
        }

        #endregion

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

    public static GameTelemetry WithFocusedVehicle(this GameTelemetry gt, Func<Vehicle, Vehicle?> f)
    {
        if (gt.FocusedVehicle is null)
            return gt;
        return gt with { FocusedVehicle = f(gt.FocusedVehicle) };
    }
    
    public static GameTelemetry WithFocusedVehicleFlags(this GameTelemetry gt, Func<VehicleFlags, VehicleFlags> f)
    {
        return WithFocusedVehicle(gt, v => v with { Flags  = f(v.Flags) });
    }

    public static GameTelemetry WithPlayer(this GameTelemetry gt, Func<Player, Player> f)
    {
        if (gt.Player is null)
            return gt;
        return gt with { Player = f(gt.Player) };
    }

    public static GameTelemetry WithPlayerWarnings(this GameTelemetry gt, Func<PlayerWarnings, PlayerWarnings> f)
    {
        return gt.WithPlayer(p => p with { Warnings = f(p.Warnings) });
    }
}