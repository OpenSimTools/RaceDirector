namespace RaceDirector.Remote.Networking.Server;

public interface IWsServer<TOut, out TIn> : IStartableConsumer<TOut>, IProducer<TIn>
{
    int Port { get; }

    event MessageHandler<TIn, TOut> MessageHandler;

    /// <summary>
    /// Multicasts data to all clients connected. Serialisation depends on the endpoint that
    /// clients are connected to.
    /// </summary>
    /// <param name="message">Message</param>
    void WsMulticastAsync(TOut message);

    /// <summary>
    /// Multicasts data to all connected clients matching a condition.
    /// Serialisation depends on the endpoint that clients are connected to.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="condition">Should this session be sent the message?</param>
    bool WsMulticastAsync(TOut message, Func<ISession<TOut>, bool> condition);
}