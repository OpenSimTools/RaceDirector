namespace RaceDirector.Remote.Networking.Server;

public interface IWsServer<in TOut, out TIn> : IStartableConsumer<TOut>, IProducer<TIn>
{
    event MessageHandler<TIn> MessageHandler;

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
    void WsMulticastAsync(TOut message, Func<ISession, bool> condition);
}