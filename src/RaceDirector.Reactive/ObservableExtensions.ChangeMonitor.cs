using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace RaceDirector.Reactive;

public static partial class ObservableExtensions
{
    /// <summary>
    /// Monitor an observable for changes. It produces an empty observable if changes
    /// were observed within the timeout, or a fallback observable otherwise. 
    /// </summary>
    /// <param name="observable">What to monitor for changes.</param>
    /// <param name="nochange">Observable to concatenate if no change was observed.</param>
    /// <param name="timeout">Timeout to monitor for changes.</param>
    /// <param name="scheduler">Optional scheduler for tests.</param>
    /// <param name="logger">Optional logger.</param>
    /// <typeparam name="TChange">Type monitored for changes.</typeparam>
    /// <typeparam name="TOut">Output type.</typeparam>
    /// <returns>Empty observable if changes were observed, <paramref name="nochange"/> otherwise.</returns>
    public static IObservable<TOut> IfNoChange<TChange, TOut>(this IObservable<TChange> observable,
        IObservable<TOut> nochange, TimeSpan timeout, IScheduler? scheduler = null, ILogger? logger = null) =>
        observable.WaitForChange(timeout, scheduler, logger)
            .Where(ItIsFalse)
            .SelectMany(_ =>
            {
                logger?.LogTrace("Timed out waiting for change");
                return nochange;
            });

    /// <summary>
    /// Monitor an observable for changes. It produces a single boolean element
    /// representing if the observable changed within the specified timeout.  
    /// </summary>
    /// <param name="observable">What to monitor for changes.</param>
    /// <param name="timeout">Timeout to monitor for changes.</param>
    /// <param name="scheduler">Optional scheduler for tests.</param>
    /// <param name="logger">Optional logger.</param>
    /// <typeparam name="T">Type monitored for changes.</typeparam>
    /// <returns>
    /// An observable with a single boolean element. True as soon as change
    /// is observed, or false when it times out.
    /// </returns>
    public static IObservable<bool> WaitForChange<T>(this IObservable<T> observable,
        TimeSpan timeout, IScheduler? scheduler = null, ILogger? logger = null) =>
        observable
            .Where(_ => _ is not null)
            .CompareWithFirst((ti, t0) =>
            {
                var ret = !ti!.Equals(t0); // Null values are already removed above
                logger?.LogTrace("Checking if {Ti} changed from {T0}: {Ret}", ti, t0, ret);
                return ret;
            })
            .WaitTrue(timeout, scheduler)
            .Concat(Observable.Return(false)) // Fallback if not enough elements
            .Take(1);

    /// <summary>
    /// Waits for a boolean observable to produce a true value within a timeout.
    /// </summary>
    /// <param name="observable">Boolean observable to monitor.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="scheduler">Optional scheduler for tests.</param>
    /// <returns>
    /// A single true element as soon as the observable produces a true
    /// element, otherwise false when it times out.
    /// </returns>
    public static IObservable<bool> WaitTrue(this IObservable<bool> observable, TimeSpan timeout, IScheduler? scheduler) =>
        TimerEx(timeout, scheduler)
            .Select(_ => false)
            .Amb(observable.SkipWhile(ItIsFalse))
            .Take(1);
    
    /// <summary>
    /// Returns the result of applying the predicate to each element and the first.
    /// </summary>
    /// <param name="observable">Observable to monitor.</param>
    /// <param name="predicate">Comparison predicate.</param>
    /// <typeparam name="T">Type of the observable.</typeparam>
    /// <returns></returns>
    public static IObservable<bool> CompareWithFirst<T>(this IObservable<T> observable,
        Func<T, T, bool> predicate) =>
        observable.Take(1)
            .SelectMany(t0 => observable.Select(ti => predicate(ti, t0)));

    private static bool ItIsFalse(bool value) => !value;

    /// <summary>
    /// Convenient observable builder to output elements at determined intervals until a change is observed.
    /// </summary>
    public static ChangeMonitorBuilder<TIn, TOut> MonitorChanges<TIn, TOut>(this IObservable<TIn> changes,
        TimeSpan timeout, IScheduler? scheduler = null, ILogger? logger = null) =>
        new(changes, timeout, scheduler, logger);

    public class ChangeMonitorBuilder<TIn, TOut>
    {
        private readonly IObservable<TIn> _observable;
        private readonly TimeSpan _timeout;
        private readonly IScheduler? _scheduler;
        private readonly ILogger? _logger;
    
        private readonly Stack<List<TOut>> _stack = new();

        internal ChangeMonitorBuilder(IObservable<TIn> observable, TimeSpan timeout, IScheduler? scheduler, ILogger? logger)
        {
            _observable = observable;
            _timeout = timeout;
            _scheduler = scheduler;
            _logger = logger;
            _stack.Push(new List<TOut>());
        }

        /// <summary>
        /// Produce elements without waiting for changes.
        /// </summary>
        public ChangeMonitorBuilder<TIn, TOut> Produce(params TOut[] values)
        {
            _stack.Peek().AddRange(values);
            return this;
        }

        /// <summary>
        /// Produce elements if no change was observed.
        /// </summary>
        public ChangeMonitorBuilder<TIn, TOut> OrProduce(params TOut[] values)
        {
            _stack.Push(values.ToList());
            return this;
        }

        /// <summary>
        /// Produces elements if all attempts failed.
        /// </summary>
        public IObservable<TOut> OrEndWith(params TOut[] values) =>
            OrEndWith(values.ToObservable());

        /// <summary>
        /// Produces an error if all attempts failed.
        /// </summary>
        public IObservable<TOut> OrEndWith(Exception exception) =>
            OrEndWith(Observable.Throw<TOut>(exception));

        private IObservable<TOut> OrEndWith(IObservable<TOut> fallback)
        {
            IObservable<TOut> ret = fallback;
            while (_stack.TryPop(out var current))
            {
                ret = current.ToObservable().Concat(_observable.IfNoChange(ret, _timeout, _scheduler, _logger));
            }
            return ret;
        }
    }
}