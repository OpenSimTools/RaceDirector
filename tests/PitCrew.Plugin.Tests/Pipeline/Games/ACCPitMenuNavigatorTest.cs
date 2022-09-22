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
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(TelemetryTicks, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(TelemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(7, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }

    [Fact]
    public void AddFuelIfCurrentIsLower()
    {
        var telemetryFuel = 5;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(TelemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel + 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    [Fact]
    public void RemovesFuelIfCurrentIsHigher()
    {
        var telemetryFuel = 5;

        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext(TelemetryTicks, TelemetryWithPitFuelToAdd(ICapacity.FromL(telemetryFuel))),
            OnCompleted<IGameTelemetry>(TelemetryTicks + 1)
        );

        var output = _testScheduler
            .Start(() => _pmn.SetFuel(telemetryFuel - 2, gameTelemetryObservable, NullLogger.Instance));

        output.Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    #endregion
    
    #region GoToKnownTireState
    
    [Fact]
    public void GoToKnownTireStateTireChangeOpenAndDrySet()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
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
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 1 * TimeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 1 * TimeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeClosedAndDry()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 2 * TimeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 2 * TimeoutTicks + telemetryOffset)
        );
    }
    
    [Fact]
    public void GoToKnownTireStateTireChangeClosedAndWet()
    {
        const int telemetryOffset = 5;
        
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnNext(0, TelemetryWithTireSet(1)),
            OnNext(Subscribed + 3 * TimeoutTicks + telemetryOffset, TelemetryWithTireSet(2)),
            OnCompleted<IGameTelemetry>(Disposed)
        );
        var replySubject = new ReplaySubject<IGameTelemetry>(1);
        gameTelemetryObservable.Subscribe(replySubject);

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(replySubject)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + 3 * TimeoutTicks + telemetryOffset)
        );
    }

    [Fact]
    public void GoToKnownTireStateSomethingWentWrong()
    {
        var gameTelemetryObservable = _testScheduler.CreateHotObservable(
            OnCompleted<IGameTelemetry>(Disposed)
        );

        _testScheduler.Start(() =>
            _pmn.GoToKnownTireState(gameTelemetryObservable)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuDown),
            OnNext(Subscribed + 0, GameAction.PitMenuRight),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 1 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + 2 * TimeoutTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuUp),
            OnNext(Subscribed + 3 * TimeoutTicks, GameAction.PitMenuRight),
            OnError<GameAction>(Subscribed + 4 * TimeoutTicks, _ => true)
        );
    }
    
    #endregion

    #region SetTires

    [Fact]
    public void SetTiresDoesNotChangeAnythingIfNotPresentInTelemetry()
    {
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(TelemetryTicks, GameTelemetry.Empty),
            OnCompleted<IGameTelemetry>(Disposed)
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(
                psrTireSet: 1,
                psrFrontLeftKpa: 2, psrFrontRightKpa: 3, psrRearLeftKpa: 4, psrRearRightKpa: 5,
                gameTelemetryObservable, NullLogger.Instance
            )
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }

    [Fact]
    public void SetTiresDoesNotChangeAnythingIfNotRequested()
    {
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(TelemetryTicks, TelemetryWithTires(
                tireSet: 0,
                tirePressures: new [] {
                    new [] { IPressure.FromPsi(2.3), IPressure.FromPsi(3.1) },
                    new [] { IPressure.FromPsi(4.3), IPressure.FromPsi(5.1) }
                })),
            OnCompleted<IGameTelemetry>(Disposed)
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(
                psrTireSet: null,
                psrFrontLeftKpa: null, psrFrontRightKpa: null, psrRearLeftKpa: null, psrRearRightKpa: null,
                gameTelemetryObservable, NullLogger.Instance
            )
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }
    
    [Fact]
    public void SetTiresAppliesTheDifference()
    {
        
        var gameTelemetryObservable = _testScheduler.CreateColdObservable(
            OnNext<IGameTelemetry>(TelemetryTicks, TelemetryWithTires(
                tireSet: 0,
                tirePressures: new [] {
                    new [] { IPressure.FromPsi(2.3), IPressure.FromPsi(3.1) },
                    new [] { IPressure.FromPsi(4.3), IPressure.FromPsi(5.1) }
                })),
            OnCompleted<IGameTelemetry>(Disposed)
        );

        _testScheduler.Start(() =>
            _pmn.SetTires(
                psrTireSet: 1,
                psrFrontLeftKpa: IPressure.FromPsi(2.2).Kpa,
                psrFrontRightKpa: IPressure.FromPsi(3.2).Kpa,
                psrRearLeftKpa: IPressure.FromPsi(4.2).Kpa,
                psrRearRightKpa: IPressure.FromPsi(5.2).Kpa,
                gameTelemetryObservable, NullLogger.Instance
            )
        ).Messages.AssertEqual(
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuLeft),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuDown),
            OnNext(Subscribed + TelemetryTicks, GameAction.PitMenuRight),
            OnCompleted<GameAction>(Subscribed + TelemetryTicks)
        );
    }

    #endregion
    
    #region Test Setup

    private const int TimeoutTicks = 13;
    private const int TelemetryTicks = 3;
    
    private readonly TestScheduler _testScheduler;
    private readonly ACCPitMenuNavigator _pmn;

    public ACCPitMenuNavigatorTest()
    {
        _testScheduler = new TestScheduler();
        _pmn = new ACCPitMenuNavigator(TimeSpan.FromTicks(TimeoutTicks), _testScheduler);
    }

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