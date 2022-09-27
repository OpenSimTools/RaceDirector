using System.Collections.Concurrent;

namespace RaceDirector.Remote.Networking.Client;

public class SyncWsClient<TOut, TIn> : WsClient<TOut, TIn>
{
    private readonly BlockingCollection<TIn> _received;
    private readonly TimeSpan _timeout;

    public SyncWsClient(string uri, Codec<TOut, TIn> codec, TimeSpan timeout) : this(new Uri(uri), codec, TimeSpan.Zero, timeout) { }
    
    public SyncWsClient(string uri, Codec<TOut, TIn> codec, TimeSpan throttling, TimeSpan timeout) : this(new Uri(uri), codec, throttling, timeout) { }

    public SyncWsClient(Uri uri, Codec<TOut, TIn> codec, TimeSpan throttling, TimeSpan timeout) : base(uri, codec, throttling)
    {
        _received = new BlockingCollection<TIn>();
        _timeout = timeout;
        MessageHandler += message => _received.Add(message);
    }

    public bool ConnectAndWait()
    {
        return Connect() && Connected.Wait(_timeout);
    }

    public TIn Next()
    {
        if (!_received.TryTake(out var body, _timeout))
            throw new TimeoutException("Message not received within timeout");
        return body;
    }

    public bool NextIsAvailable() => Available() > 0;

    public int Available() => _received.Count;
}