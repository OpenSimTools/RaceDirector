using System.Reactive.Subjects;
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

namespace PitCrew.Plugin.Tests.Pipeline.Games;

[UnitTest]
public class ACCPitMenuNavigatorTest : ReactiveTest
{
    // TODO Two SetStrategy tests success and failure
    
    #region SetFuel

    [Fact]
    public void DontChangeFuelIfNotPresentInTelemetry()
    {
        var telemetryTicks = 3;
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(telemetryTicks, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(telemetryTicks + 1)
        );

        var pmn = new ACCPitMenuNavigator();

        var output = _testScheduler
            .Start(() => pmn.SetFuel(7, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnCompleted<GameAction>(Subscribed + telemetryTicks)
        );
    }

    [Fact]
    public void AddFuelIfCurrentIsLower()
    {
        var telemetryFuel = 5;
        var telemetryTicks = 3;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(telemetryTicks, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(telemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel + 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + telemetryTicks)
        );
    }
    
    [Fact]
    public void RemovesFuelIfCurrentIsHigher()
    {
        var telemetryFuel = 5;
        var telemetryTicks = 3;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(telemetryTicks, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(telemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel - 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuLeft),
            OnCompleted<GameAction>(Subscribed + telemetryTicks)
        );
    }
    
    #endregion
    
    #region GoToKnownTireState
    
    [Fact]
    public void GoToKnownTireStateTireChangeOpenAndDrySet()
    {
        const int timeoutTicks = 13;
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.GoToKnownTireState(replySubject, TimeSpan.FromTicks(timeoutTicks), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeOpenAndWet()
    {
        const int timeoutTicks = 13;
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 1 * timeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.GoToKnownTireState(replySubject, TimeSpan.FromTicks(timeoutTicks), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 1 * timeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeClosedAndDry()
    {
        const int timeoutTicks = 13;
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 2 * timeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.GoToKnownTireState(replySubject, TimeSpan.FromTicks(timeoutTicks), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * timeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * timeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 2 * timeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeClosedAndWet()
    {
        const int timeoutTicks = 13;
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 3 * timeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.GoToKnownTireState(replySubject, TimeSpan.FromTicks(timeoutTicks), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * timeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * timeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 3 * timeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 3 * timeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 3 * timeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 3 * timeoutTicks + telemetryOffset)
        );
    }

    [Fact]
    public void GoToKnownTireStateSomethingWentWrong()
    {
        const int timeoutTicks = 13;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnCompleted<IGameTelemetry>(Disposed)
        );

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.GoToKnownTireState(gameTelemetryObservable, TimeSpan.FromTicks(timeoutTicks), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * timeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * timeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * timeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 3 * timeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 3 * timeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 3 * timeoutTicks, GameAction.PitMenuRight),
            OnError<GameAction>(Subscribed + 4 * timeoutTicks, _ => true)
        );
    }
    
    #endregion

    #region SetTires

    [Fact]
    public void SetTiresDoesNotChangeAnythingIfNotPresentInTelemetry()
    {
        var telemetryTicks = 3;
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(telemetryTicks, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(Disposed)
        );

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.SetTires(
                psrTireSet: 1,
                psrFrontLeftKpa: 2, psrFrontRightKpa: 3, psrRearLeftKpa: 4, psrRearRightKpa: 5,
                gameTelemetryObservable, NullLogger.Instance
            )
        ).Messages.AssertEqual(
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + telemetryTicks)
        );
    }

    [Fact]
    public void SetTiresDoesNotChangeAnythingIfNotRequested()
    {
        var telemetryTicks = 3;
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(telemetryTicks, TelemetryWithTires(
                tireSet: 0,
                tirePressures: new [] {
                    new [] { IPressure.FromPsi(2.3), IPressure.FromPsi(3.1) },
                    new [] { IPressure.FromPsi(4.3), IPressure.FromPsi(5.1) }
                })),
            OnCompleted<IGameTelemetry>(Disposed)
        );

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.SetTires(
                psrTireSet: null,
                psrFrontLeftKpa: null, psrFrontRightKpa: null, psrRearLeftKpa: null, psrRearRightKpa: null,
                gameTelemetryObservable, NullLogger.Instance
            )
        ).Messages.AssertEqual(
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + telemetryTicks)
        );
    }
    
    [Fact]
    public void SetTiresAppliesTheDifference()
    {
        var telemetryTicks = 3;
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(telemetryTicks, TelemetryWithTires(
                tireSet: 0,
                tirePressures: new [] {
                    new [] { IPressure.FromPsi(2.3), IPressure.FromPsi(3.1) },
                    new [] { IPressure.FromPsi(4.3), IPressure.FromPsi(5.1) }
                })),
            OnCompleted<IGameTelemetry>(Disposed)
        );

        var pmn = new ACCPitMenuNavigator();

        _testScheduler.Start(() =>
            pmn.SetTires(
                psrTireSet: 1,
                psrFrontLeftKpa: IPressure.FromPsi(2.2).Kpa,
                psrFrontRightKpa: IPressure.FromPsi(3.2).Kpa,
                psrRearLeftKpa: IPressure.FromPsi(4.2).Kpa,
                psrRearRightKpa: IPressure.FromPsi(5.2).Kpa,
                gameTelemetryObservable, NullLogger.Instance
            )
        ).Messages.AssertEqual(
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + telemetryTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + telemetryTicks)
        );
    }

    #endregion
    
    #region Test Setup

    private readonly TestScheduler _testScheduler = new();
    private readonly ACCPitMenuNavigator _pmn = new();

    private IGameTelemetry TelemetryWithPitFuelToAdd(ICapacity? fuelToAdd) =>
        TelemetryWithPitMenu(new PitMenu(
            FocusedItem: PitMenuFocusedItem.Unavailable,
            SelectedItems: 0,
            FuelToAdd: fuelToAdd,
            TireSet: null,
            TirePressures: Array.Empty<IPressure[]>()
        ));
    
    private IGameTelemetry TelemetryWithTireSet(int? tireSet) =>
        TelemetryWithTires(tireSet, Array.Empty<IPressure[]>());

    private IGameTelemetry TelemetryWithTires(int? tireSet, IPressure[][] tirePressures) =>
        TelemetryWithPitMenu(new PitMenu(
            FocusedItem: PitMenuFocusedItem.Unavailable,
            SelectedItems: 0,
            FuelToAdd: null,
            TireSet: tireSet,
            TirePressures: tirePressures
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