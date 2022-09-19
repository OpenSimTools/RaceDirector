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
            // .Concat(GoToKnownTireState(gto))
            // .Concat(SetTires(psr.TireSet, psr.FrontTires, psr.RearTires, gto, logger))
            // .Append(GameAction.PitMenuDown) // Leave selection to brakes entry
            // .Catch(Observable.Return(GameAction.PitMenuOpen)); // Leave selection to top on error

    private IObservable<GameAction> SetFuel(double? requestedFuelToAdd,
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
    private IObservable<GameAction> GoToKnownTireState(IObservable<IGameTelemetry> gto) =>
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
                                        Observable.Throw<GameAction>(new Exception("Something went wrong")
                                        )
                                    ))
                            ))
                    ))
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

public static class ObservablePitStrategyExt
{
    public static IObservable<GameAction> IfFieldDidNotChange<TOuter, TInner>(this IObservable<TOuter> observable,
        Func<TOuter, TInner?> extractField, TimeSpan timeout, IObservable<GameAction> then) =>
        observable
            .WaitForFieldChange(extractField, timeout)
            .Where(cond => !cond)
            .SelectMany(_ => then);

    public static IObservable<bool> WaitForFieldChange<TOuter, TInner>(this IObservable<TOuter> observable,
        Func<TOuter, TInner?> extractField, TimeSpan timeout) =>
        observable
            .CompareWithFirst(FieldNotNullAndNotEqual(extractField))
            .WaitTrue(timeout);

    // Returns a single true element if successful within the timeout, false otherwise
    public static IObservable<bool> WaitTrue(this IObservable<bool> observable, TimeSpan timeout) =>
        observable.SkipWhile(_ => !_).Take(1).Amb(Observable.Timer(timeout).Select(_ => false));

    // Returns the result of applying the predicate to each element and the first
    public static IObservable<bool> CompareWithFirst<T>(this IObservable<T> observable,
        Func<T, T, bool> predicate) =>
        observable.Take(1)
            .SelectMany(t0 => observable.Select(ti => predicate(ti, t0)));

    public static Func<TOuter, TOuter, bool> FieldNotNullAndNotEqual<TOuter, TInner>(Func<TOuter, TInner?> extract) =>
        (t1, t2) => NotNullAndNotEqual(extract(t1), extract(t2));

    public static bool NotNullAndNotEqual<T>(T? t1, T? t2) =>
        t1 is not null && t2 is not null && !t1.Equals(t2);
}