using System.Reactive;

namespace RaceDirector.Remote.Pipeline;

/// <summary>
/// Starts and stops remote publishers based on trigger input.
/// Publishes to all on data input.
/// </summary>
/// <typeparam name="TTrigger">Trigger type</typeparam>
/// <typeparam name="TData">Data type</typeparam>
public abstract class WsNodeBase<TTrigger, TData>
{
    protected readonly IObserver<TTrigger> TriggerObserver;
    protected readonly IObserver<TData> DataObserver;

    protected WsNodeBase(IEnumerable<IRemotePublisher<TData>> publishers)
    {
        TriggerObserver = Observer.Create<TTrigger>(trigger =>
        {
            if (PublisherShouldStart(trigger))
                foreach (var s in publishers) s.Start();
            else
                foreach (var s in publishers) s.Stop();
        });
        DataObserver = Observer.Create<TData>(data =>
        {
            foreach (var s in publishers) s.PublishAsync(data);
        });
    }

    /// <summary>
    /// Returns if the trigger data should start ot stop the publishers.
    /// </summary>
    /// <param name="trigger"></param>
    /// <returns></returns>
    protected abstract bool PublisherShouldStart(TTrigger trigger);
}