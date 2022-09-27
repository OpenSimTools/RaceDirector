using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.Reactive;

public static partial class ObservableExtensions
{
    /// <summary>
    /// Same as <see cref="M:Delay"/>, but with an optional scheduler for testing.
    /// </summary>
    public static IObservable<T> DelayEx<T>(this IObservable<T> observable, TimeSpan delay,
        IScheduler? scheduler = null) =>
        scheduler is null ? observable.Delay(delay) : observable.Delay(delay, scheduler);

    /// <summary>
    /// Same as <see cref="M:Timer"/>, but with an optional scheduler for testing.
    /// </summary>
    public static IObservable<long> TimerEx(TimeSpan timeout, IScheduler? scheduler) =>
        scheduler is null ? Observable.Timer(timeout) : Observable.Timer(timeout, scheduler);

}