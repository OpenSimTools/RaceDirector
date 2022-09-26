using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.Reactive;

// Having to add the scheduler for testing observables is just insane!
public static class ObservableExtensions
{
    /// <summary>
    /// Variant of <see cref="M:SelectMany"/> that concatenates the returned observables instead of executing them
    /// in parallel.
    /// </summary>
    public static IObservable<TResult> SelectManyConcat<TSource, TResult>(this IObservable<TSource> observable,
        Func<TSource, IObservable<TResult>> func) =>
        observable.Select(func).Concat();

    /// <summary>
    /// Like <see cref="M:SelectMany"/> but the selector terminates when the source emits the next element.
    /// </summary>
    public static IObservable<TResult> SelectManyUntilNext<TSource, TResult>(this IObservable<TSource> source,
        Func<TSource, IObservable<TResult>> selector)
    {
        return source.SelectMany(s => selector(s).TakeUntil(source));
    }
    
    public static IObservable<T> SpacedBy<T>(this IObservable<T> observable, TimeSpan delay,
        IScheduler scheduler) => _SpacedBy(observable, delay, scheduler);

    public static IObservable<T> SpacedBy<T>(this IObservable<T> observable, TimeSpan delay) =>
        _SpacedBy(observable, delay, null);
    
    private static IObservable<T> _SpacedBy<T>(IObservable<T> observable, TimeSpan delay, IScheduler? scheduler) =>
        observable.SelectManyConcat(_ =>
            Observable.Return(_).Concat(
                scheduler is null ?
                    Observable.Empty<T>().Delay(delay) :
                    Observable.Empty<T>().Delay(delay, scheduler)
            )
        );

    public static IObservable<TOut> IfNoChange<TIn, TOut>(this IObservable<TIn> observable,
        IObservable<TOut> then, TimeSpan timeout, IScheduler? scheduler = null) =>
        observable
            .WaitForChange(timeout, scheduler)
            .Where(ItIsFalse)
            .SelectMany(_ => then);

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