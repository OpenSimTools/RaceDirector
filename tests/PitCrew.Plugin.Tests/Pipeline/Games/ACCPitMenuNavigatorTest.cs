using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline.Games;
using Xunit;
using Xunit.Categories;
using PitMenu = RaceDirector.Pipeline.Telemetry.PitMenu;

namespace PitCrew.Plugin.Tests.Pipeline.Games;

[UnitTest]
public class ACCPitMenuNavigatorTest : ReactiveTest
{
    [Fact]
    public void DontChangeFuelIfNotPresentInTelemetry()
    {
        var telemetryTick = 3;
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(telemetryTick, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(telemetryTick + 1)
        );

        var pmn = new ACCPitMenuNavigator();

        var output = _testScheduler
            .Start(() => pmn.SetFuel(7, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnCompleted<GameAction>(Subscribed + telemetryTick)
        );
    }

    [Fact]
    public void AddFuelIfCurrentIsLower()
    {
        var telemetryFuel = 5;
        var telemetryTick = 3;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(telemetryTick, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(telemetryTick + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel + 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + telemetryTick, GameAction.PitMenuRight),
            OnNext(Subscribed + telemetryTick, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + telemetryTick)
        );
    }
    
    [Fact]
    public void RemovesFuelIfCurrentIsHigher()
    {
        var telemetryFuel = 5;
        var telemetryTick = 3;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(telemetryTick, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(telemetryTick + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel - 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + telemetryTick, GameAction.PitMenuLeft),
            OnNext(Subscribed + telemetryTick, GameAction.PitMenuLeft),
            OnCompleted<GameAction>(Subscribed + telemetryTick)
        );
    }

    #region Test Setup

    private readonly TestScheduler _testScheduler = new TestScheduler();
    private readonly ACCPitMenuNavigator _pmn = new ACCPitMenuNavigator();

    private IGameTelemetry TelemetryWithPitFuelToAdd(ICapacity? fuelToAdd) =>
        TelemetryWithPitMenu(new PitMenu(
            FocusedItem: PitMenuFocusedItem.Unavailable,
            SelectedItems: 0,
            FuelToAdd: fuelToAdd,
            TireSet: null,
            TirePressures: Array.Empty<IPressure[]>()
        ));
    
    private IGameTelemetry TelemetryWithPitMenu(IPitMenu pitMenu)
    {
        var telemetryMock = new Mock<IGameTelemetry>();
        telemetryMock.SetupGet(_ => _.Player)
            .Returns(() =>
            {
                var playerMock = new Mock<IPlayer>();
                playerMock.SetupGet(_ => _.PitMenu)
                    .Returns(() => pitMenu);
                return playerMock.Object;
            });
        return telemetryMock.Object;
    }
    
    #endregion
}