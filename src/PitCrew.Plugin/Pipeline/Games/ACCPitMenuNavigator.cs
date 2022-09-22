using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline.Games;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Protocol;

namespace RaceDirector.PitCrew.Pipeline.Games;

public class ACCPitMenuNavigator : IGamePitMenuNavigator
{
    public string GameName => Names.ACC;

    private TimeSpan _timeout = TimeSpan.FromMilliseconds(500); // TODO

    public IObservable<GameAction> SetStrategy(IPitStrategyRequest psr,
        IObservable<IGameTelemetry> gto, ILogger logger) =>
        new[] {GameAction.PitMenuOpen, GameAction.PitMenuDown, GameAction.PitMenuDown}.ToObservable()
            .Concat(SetFuel(psr.FuelToAddL, gto, logger));
            // TODO TEST EACH PART SEPARATELY ONLY :FACEPALM:
            // .Concat(GoToKnownTireState(gto))
            // .Concat(SetTires(psr.TireSet, psr.FrontTires, psr.RearTires, gto, logger))
            // .Append(GameAction.PitMenuDown) // Leave selection to brakes entry
            // .Catch(Observable.Return(GameAction.PitMenuOpen)); // Leave selection to top on error

    public IObservable<GameAction> SetFuel(double? requestedFuelToAdd,
        IObservable<IGameTelemetry> gameTelemetryObservable, ILogger logger) =>
        gameTelemetryObservable.Take(1).SelectMany(t =>
        {
            var fuelInMenu = t.Player?.PitMenu.FuelToAdd?.L;
            if (fuelInMenu is null || requestedFuelToAdd is null)
                return Observable.Empty<GameAction>();
            var fuelToAdd = (int) (requestedFuelToAdd - fuelInMenu);
            logger.LogDebug("Fuel change: {}", fuelToAdd);
            return fuelToAdd >= 0
                ? Observable.Repeat(GameAction.PitMenuRight, fuelToAdd)
                : Observable.Repeat(GameAction.PitMenuLeft, -fuelToAdd);
        });

    // TODO Bad implementation, replace with the one below after tests
    private IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto, IScheduler? scheduler = null) =>
        new[] {GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuRight}.ToObservable()
            .Concat(gto.IfFieldDidNotChange(
                gt => gt.Player?.PitMenu.TireSet,
                _timeout,
                new[] {GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight}.ToObservable()
                    .Concat(gto.IfFieldDidNotChange(
                        gt => gt.Player?.PitMenu.TireSet,
                        _timeout,
                        new[] {GameAction.PitMenuDown, GameAction.PitMenuRight}.ToObservable()
                            .Concat(gto.IfFieldDidNotChange(
                                gt => gt.Player?.PitMenu.TireSet,
                                _timeout,
                                new[] {GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight}.ToObservable()
                                    .Concat(gto.IfFieldDidNotChange(
                                        gt => gt.Player?.PitMenu.TireSet,
                                        _timeout,
                                        Observable.Throw<GameAction>(new Exception("Something went wrong")),
                                        scheduler
                                    )),
                                scheduler
                            )),
                        scheduler
                    )),
                scheduler
            ));

    // TODO Better implementation with builder pattern
    // private IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto)
    // {
    //     var tireSet = (IGameTelemetry gt) => gt.Player?.PitMenu.TireSet;
    //
    //     return CheckingTelemetry(gto)
    //         .Send(GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuRight)
    //         .Changed(tireSet).OrSend(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
    //         .Changed(tireSet).OrSend(GameAction.PitMenuDown, GameAction.PitMenuRight)
    //         .Changed(tireSet).OrSend(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
    //         .Changed(tireSet).OrFailWith(new Exception("Shit went wrong"));
    // }

    private IObservable<GameAction> SetTires(int? psrTireSet, IPitMenuTires? psrFrontTires, IPitMenuTires? psrRearTires, IObservable<IGameTelemetry> gto, ILogger logger)
    {
        // TODO implement it
        return Observable.Empty<GameAction>();
    }
    
}