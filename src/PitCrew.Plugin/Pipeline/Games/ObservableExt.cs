using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.PitCrew.Pipeline.Games;

public static class ObservableExt
{
    // Having to add the scheduler for testing observables is just insane!
    public static IObservable<TOut> IfNoChange<TIn, TOut>(this IObservable<TIn> observable,
        IObservable<TOut> then, TimeSpan timeout, IScheduler? scheduler = null) =>
        observable
            .WaitForChange(timeout, scheduler)
            .Where(ItIsFalse)
            .SelectMany(_ => then);

    // Having to add the scheduler for testing observables is just insane!
    public static IObservable<bool> WaitForChange<T>(this IObservable<T> observable,
        TimeSpan timeout, IScheduler? scheduler = null) =>
        observable
            .Where(_ => _ is not null)
            .CompareWithFirst((a, b) => !a!.Equals(b))
            .WaitTrue(timeout, scheduler)
            .Concat(Observable.Return(false)) // Fallback if not enough elements
            .Take(1);

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

    private static bool ItIsFalse(bool value) => !value;
}