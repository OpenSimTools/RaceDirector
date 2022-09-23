﻿using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RaceDirector.PitCrew.Pipeline.Games;

public class ChangeMonitor<TIn, TOut>
{
    private readonly IObservable<TIn> _observable;
    private readonly TimeSpan _timeout;
    private readonly IScheduler? _scheduler;
    
    private readonly Stack<List<TOut>> _stack;

    public static ChangeMonitor<TIn, TOut> MonitorChanges(IObservable<TIn> changes,
        TimeSpan timeout, IScheduler? scheduler = null) => new(changes, timeout, scheduler);

    private ChangeMonitor(IObservable<TIn> observable, TimeSpan timeout, IScheduler? scheduler = null)
    {
        _observable = observable;
        _timeout = timeout;
        _scheduler = scheduler;
        _stack = new();
        _stack.Push(new List<TOut>());
    }

    public ChangeMonitor<TIn, TOut> Send(params TOut[] values)
    {
        _stack.Peek().AddRange(values);
        return this;
    }

    public ChangeMonitor<TIn, TOut> OrSend(params TOut[] values)
    {
        _stack.Push(values.ToList());
        return this;
    }

    public IObservable<TOut> OrEndWith(params TOut[] values) =>
        OrEndWith(values.ToObservable());

    public IObservable<TOut> OrEndWith(Exception exception) =>
        OrEndWith(Observable.Throw<TOut>(exception));

    private IObservable<TOut> OrEndWith(IObservable<TOut> fallback)
    {
        IObservable<TOut> ret = fallback;
        while (_stack.TryPop(out var current))
        {
            ret = current.ToObservable().Concat(_observable.IfNoChange(ret, _timeout, _scheduler));
        }
        return ret;
    }
}

public static class ChangeMonitor
{

}