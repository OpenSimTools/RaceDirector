using System.Reactive;
using RaceDirector.Remote.Networking;

namespace RaceDirector.Remote.Pipeline;

/// <summary>
/// Starts and stops remote pushers based on trigger input.
/// Pushes to all on data input.
/// </summary>
/// <typeparam name="TTrigger">Trigger type</typeparam>
/// <typeparam name="TData">Data type</typeparam>
public abstract class WsNodeBase<TTrigger, TData>
{
    protected readonly IObserver<TTrigger> TriggerObserver;
    protected readonly IObserver<TData> DataObserver;

    protected WsNodeBase(IEnumerable<IRemotePusher<TData>> pushers)
    {
        TriggerObserver = Observer.Create<TTrigger>(trigger =>
        {
            if (PusherShouldStart(trigger))
                foreach (var s in pushers) s.Start();
            else
                foreach (var s in pushers) s.Stop();
        });
        DataObserver = Observer.Create<TData>(data =>
        {
            foreach (var s in pushers) s.PushAsync(data);
        });
    }

    /// <summary>
    /// Returns if the trigger data should start ot stop the pushers.
    /// </summary>
    /// <param name="trigger"></param>
    /// <returns></returns>
    protected abstract bool PusherShouldStart(TTrigger trigger);
}