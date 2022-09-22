using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.PitCrew.Pipeline.Games;

public static class ObservableExt
{
    // Having to add the scheduler for testing observables is just insane!
    public static IObservable<TOut> IfFieldDidNotChange<TOuter, TInner, TOut>(this IObservable<TOuter> observable,
        Func<TOuter, TInner?> extractField, TimeSpan timeout, IObservable<TOut> then, IScheduler? scheduler = null) =>
        observable
            .WaitForFieldChange(extractField, timeout, scheduler)
            .Where(ItIsFalse)
            .SelectMany(_ => then);

    // Having to add the scheduler for testing observables is just insane!
    public static IObservable<bool> WaitForFieldChange<TOuter, TInner>(this IObservable<TOuter> observable,
        Func<TOuter, TInner?> extractField, TimeSpan timeout, IScheduler? scheduler = null) =>
        observable
            .CompareWithFirst(FieldNotNullAndNotEqual(extractField))
            .WaitTrue(timeout, scheduler);

    public static IObservable<bool> WaitTrue(this IObservable<bool> observable, TimeSpan timeout, IScheduler? scheduler) =>
        (scheduler is null ? Observable.Timer(timeout) : Observable.Timer(timeout, scheduler)).Select(_ => false)
            .Amb(observable.SkipWhile(ItIsFalse))
            .Take(1);

    /// <summary>
    /// Returns the result of applying the predicate to each element and the first.
    /// </summary>
    public static IObservable<bool> CompareWithFirst<T>(this IObservable<T> observable,
        Func<T, T, bool> predicate) =>
        observable.Take(1)
            .SelectMany(t0 => observable.Select(ti => predicate(ti, t0)));

    public static Func<TOuter, TOuter, bool> FieldNotNullAndNotEqual<TOuter, TInner>(Func<TOuter, TInner?> extract) =>
        (t1, t2) => NotNullAndNotEqual(extract(t1), extract(t2));

    public static bool NotNullAndNotEqual<T>(T? t1, T? t2) =>
        t1 is not null && t2 is not null && !t1.Equals(t2);

    private static bool ItIsFalse(bool value) => !value;
}