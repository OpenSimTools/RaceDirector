using RaceDirector.HUD.Server;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace RaceDirector.HUD.Pipeline;

/// <summary>
/// Starts and stops WebSocket servers based on trigger input.
/// Broadcasts to all WebSocket clients on data input.
/// </summary>
/// <typeparam name="TTrigger">Trigger type</typeparam>
/// <typeparam name="TData">Data type</typeparam>
public abstract class WebSocketNodeBase<TTrigger, TData>
{
    protected readonly IObserver<TTrigger> TriggerObserver;
    protected readonly IObserver<TData> DataObserver;

    protected WebSocketNodeBase(IEnumerable<IWsServer<TData>> servers)
    {
        TriggerObserver = Observer.Create<TTrigger>(trigger => {
            if (ServerShouldRun(trigger))
                foreach (var s in servers) s.Start();
            else
                foreach (var s in servers) s.Stop();
        });
        DataObserver = Observer.Create<TData>(data =>
        {
            foreach (var s in servers) s.Multicast(data);
        });
    }

    /// <summary>
    /// Returns if the trigger data should start ot stop the servers.
    /// </summary>
    /// <param name="trigger"></param>
    /// <returns></returns>
    protected abstract bool ServerShouldRun(TTrigger trigger);
}