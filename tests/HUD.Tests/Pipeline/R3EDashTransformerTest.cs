using HUD.Tests.TestUtils;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Plugin.HUD.Pipeline;
using System.Text.Json;
using Xunit;
using Xunit.Categories;
using System;
using RaceDirector.Pipeline.Telemetry.Physics;
using AutoBogus;

namespace HUD.Tests.Pipeline
{
    [UnitTest]
    public class R3EDashTransformerTest
    {
        [Fact]
        public void VersionInformation()
        {
            var result = ToR3EDash(f => {});

            Assert.Equal(2, result.Path("VersionMajor").GetInt32());
            Assert.Equal(10, result.Path("VersionMinor").GetInt32());
        }

        //[Property]
        //public void GameStateMenu(GameTelemetry gameTelemetry)
        //{
        //    var result = ToR3EDash(gameTelemetry with { GameState = GameState.Menu });
        //    Assert.Equal(1, result.Path("GameInMenus").GetInt32());
        //    Assert.Equal(0, result.Path("GameInReplay").GetInt32());
        //}

        //[Property]
        //public void GameStateReplay(GameTelemetry gameTelemetry)
        //{
        //    var result = ToR3EDash(gameTelemetry with { GameState = GameState.Replay });
        //    Assert.Equal(0, result.Path("GameInMenus").GetInt32());
        //    Assert.Equal(1, result.Path("GameInReplay").GetInt32());
        //}

        //[Property]
        //public Property GameStateNotMenuOrReplay(GameTelemetry gameTelemetry) => new Action(() =>
        //{
        //    var result = ToR3EDash(gameTelemetry);
        //    Assert.Equal(0, result.Path("GameInMenus").GetInt32());
        //}).When(
        //    gameTelemetry.GameState != GameState.Menu &&
        //    gameTelemetry.GameState != GameState.Replay
        //);

        //[Property]
        //public void UsingVr(GameTelemetry gameTelemetry)
        //{
        //    var result = ToR3EDash(gameTelemetry with { UsingVR = true });
        //    Assert.Equal(1, result.Path("GameUsingVr").GetInt32());

        //    result = ToR3EDash(gameTelemetry with { UsingVR = false });
        //    Assert.Equal(0, result.Path("GameUsingVr").GetInt32());
        //}

        //[Property]
        //public Property EventTrack3Sectors(GameTelemetry gameTelemetry, Double lengthM, Double s1, Double s2, Double s3) => new Action(() =>
        //{
        //    var result = ToR3EDash(gameTelemetry.WithTrack(track => track with
        //    {
        //        SectorsEnd = new IFraction<IDistance>[] {
        //            IFraction.Of(IDistance.FromM(1), s1),
        //            IFraction.Of(IDistance.FromM(1), s2),
        //            IFraction.Of(IDistance.FromM(lengthM), s3)
        //        }
        //    }));
        //    Assert.Equal(lengthM, result.Path("LayoutLength").GetDouble());
        //    Assert.Equal(s1, result.Path("SectorStartFactors", "Sector1").GetDouble());
        //    Assert.Equal(s2, result.Path("SectorStartFactors", "Sector2").GetDouble());
        //    Assert.Equal(s2, result.Path("SectorStartFactors", "Sector3").GetDouble());
        //}).When(
        //    gameTelemetry.Event is not null
        //);

        //[Property]
        //public void TestMultipleArgumentsNoAction(Double a, Double b)
        //{
        //    // Maybe I shouldn't be using property based testing at all!
        //    Assert.True(a < b);
        //}

        //[Property]
        //public void EventNotPresent(GameTelemetry gameTelemetry)
        //{
        //    var result = ToR3EDash(gameTelemetry with { Event = null });
        //    Assert.Equal(-1.0, result.Path("LayoutLength").GetDouble());
        //}

        //[Property]
        //public Property EventTrackNoSectors(GameTelemetry gameTelemetry) => new Action(() =>
        //{
        //    var result = ToR3EDash(gameTelemetry.WithTrack(track => track with {
        //        SectorsEnd = Array.Empty<IFraction<IDistance>>()
        //    }));
        //    Assert.Equal(-1.0, result.Path("LayoutLength").GetDouble());
        //}).When(
        //    gameTelemetry.Event is not null
        //);

        //[Property]
        //public Property EventTrackNot3Sectors(GameTelemetry gameTelemetry, IFraction<IDistance>[] sectorsEnd) => new Action(() =>
        //{
        //    var result = ToR3EDash(gameTelemetry.WithTrack(track => track with
        //    {
        //        SectorsEnd = sectorsEnd
        //    }));
        //    Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector1").GetDouble());
        //    Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector2").GetDouble());
        //    Assert.Equal(-1.0, result.Path("SectorStartFactors", "Sector3").GetDouble());
        //}).When(
        //    gameTelemetry.Event is not null &&
        //    sectorsEnd.Length != 3
        //);

        #region Test setup

        private static JsonDocument ToR3EDash(Action<AutoFaker<GameTelemetry>> configureFaker)
        {
            var autoFaker = new AutoFaker<GameTelemetry>();
            configureFaker(autoFaker);
            var bytes = R3EDashTransformer.ToR3EDash(autoFaker.Generate());
            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonDocument.Parse(jsonString);
        }

        #endregion
    }
}

static class GameTelemetryExensions
{
    public static GameTelemetry WithTrack(this GameTelemetry gt, Func<TrackLayout, TrackLayout> f)
    {
        if (gt.Event is null)
            return gt;
        return gt with
        {
            Event = gt.Event with
            {
                Track = f(gt.Event.Track)
            }
        };
    }

    public static GameTelemetry WithSectorsEnd(this GameTelemetry gt, IFraction<IDistance>[] sectorsEnd)
    {
        return gt.WithTrack(track => track with { SectorsEnd = sectorsEnd });
    }
}