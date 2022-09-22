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
            .Concat(SetFuel(psr.FuelToAddL, gto, logger))
            .Concat(GoToKnownTireState(gto, _timeout))
            .Concat(SetTires(psr.TireSet, psr.FrontTires, psr.RearTires, gto, logger))
            .Append(GameAction.PitMenuDown) // Leave selection to brakes entry
            .Catch(Observable.Return(GameAction.PitMenuOpen)); // Leave selection to top on error

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
    public IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto, TimeSpan timeout,
        IScheduler? scheduler = null)
    {
        var tireSetTelemetry = gto.Select(_ => _.Player?.PitMenu.TireSet);
        return new[] {GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuRight}.ToObservable()
            .Concat(tireSetTelemetry.IfNoChange(
                new[] {GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight}.ToObservable()
                    .Concat(tireSetTelemetry.IfNoChange(
                        new[] {GameAction.PitMenuDown, GameAction.PitMenuRight}.ToObservable()
                            .Concat(tireSetTelemetry.IfNoChange(
                                new[] {GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight}.ToObservable()
                                    .Concat(tireSetTelemetry.IfNoChange(
                                        Observable.Throw<GameAction>(new Exception("Pit menu in unknown state")),
                                        timeout, scheduler
                                    )),
                                timeout, scheduler
                            )),
                        timeout, scheduler
                    )),
                timeout, scheduler
            ));
    }

    // TODO Better implementation with builder pattern
    // public IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto)
    // {
    //     var tireSetTelemetry = gto.Select(_ => _.Player?.PitMenu.TireSet);
    //
    //     return WaitForChange(tireSetTelemetry)
    //         .Send(GameAction.PitMenuDown, GameAction.PitMenuDown, GameAction.PitMenuRight)
    //         .IfUnchangedSend(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
    //         .IfUnchangedSend(GameAction.PitMenuDown, GameAction.PitMenuRight)
    //         .IfUnchangedSend(GameAction.PitMenuLeft, GameAction.PitMenuUp, GameAction.PitMenuRight)
    //         .IfUnchangedFailWith(new Exception("Pit menu in unknown state"));
    // }

    public IObservable<GameAction> SetTires(int? psrTireSet, IPitMenuTires? psrFrontTires, IPitMenuTires? psrRearTires, IObservable<IGameTelemetry> gto, ILogger logger)
    {
        // TODO implement it
        return Observable.Empty<GameAction>();
    }
    
}