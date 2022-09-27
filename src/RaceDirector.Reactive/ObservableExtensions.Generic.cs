using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.Reactive;

public static partial class ObservableExtensions
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
    
    /// <summary>
    /// Spaces the elements produced by the observable by at least some <paramref name="delay"/>.
    /// </summary>
    public static IObservable<T> SpacedBy<T>(this IObservable<T> observable, TimeSpan delay, IScheduler? scheduler = null) =>
        observable.SelectManyConcat(_ =>
            Observable.Return(_).Concat(Observable.Empty<T>().DelayEx(delay, scheduler))
        );
}