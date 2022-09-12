using Microsoft.Reactive.Testing;
using Moq;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.PitCrew.Protocol;
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
        var testScheduler = new TestScheduler();
        var gameTelemetryObservable = testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(3, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(10)
        );

        var pmn = new ACCPitMenuNavigator();
        var psr = new PitStrategyRequest(7);

        var output = testScheduler.Start(() => pmn.SetStrategy(psr, gameTelemetryObservable));

        output.Messages.AssertEqual(
            OnNext(200, GameAction.PitMenuOpen),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(200, GameAction.PitMenuDown),
            OnCompleted<GameAction>(203)
        );
    }

    [Fact]
    public void AddFuelIfCurrentIsLower()
    {
        var testScheduler = new TestScheduler();
        var gameTelemetryObservable = testScheduler.CreateColdObservable(
            OnNext(3, TelemetryWithPitFuelToAdd(ICapacity.FromL(5))),
            OnCompleted<IGameTelemetry>(10)
        );

        var pmn = new ACCPitMenuNavigator();
        var psr = new PitStrategyRequest(7);

        var output = testScheduler.Start(() => pmn.SetStrategy(psr, gameTelemetryObservable));

        output.Messages.AssertEqual(
            OnNext(200, GameAction.PitMenuOpen),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(203, GameAction.PitMenuRight),
            OnNext(203, GameAction.PitMenuRight),
            OnCompleted<GameAction>(203)
        );
    }
    
    [Fact]
    public void RemovesFuelIfCurrentIsHigher()
    {
        var testScheduler = new TestScheduler();
        var gameTelemetryObservable = testScheduler.CreateColdObservable(
            OnNext(3, TelemetryWithPitFuelToAdd(ICapacity.FromL(5))),
            OnCompleted<IGameTelemetry>(10)
        );

        var pmn = new ACCPitMenuNavigator();
        var psr = new PitStrategyRequest(3);

        var output = testScheduler.Start(() => pmn.SetStrategy(psr, gameTelemetryObservable));

        output.Messages.AssertEqual(
            OnNext(200, GameAction.PitMenuOpen),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(200, GameAction.PitMenuDown),
            OnNext(203, GameAction.PitMenuLeft),
            OnNext(203, GameAction.PitMenuLeft),
            OnCompleted<GameAction>(203)
        );
    }

    private IGameTelemetry TelemetryWithPitFuelToAdd(ICapacity? fuelToAdd)
    {
        var telemetryMock = new Mock<IGameTelemetry>();
        telemetryMock.SetupGet(_ => _.Player)
            .Returns(() =>
            {
                var playerMock = new Mock<IPlayer>();
                playerMock.SetupGet(_ => _.PitMenu)
                    .Returns(() => new PitMenu(
                        FocusedItem: PitMenuFocusedItem.Unavailable,
                        SelectedItems: 0,
                        FuelToAdd: fuelToAdd 
                    ));
                return playerMock.Object;
            });
        return telemetryMock.Object;
    }
}