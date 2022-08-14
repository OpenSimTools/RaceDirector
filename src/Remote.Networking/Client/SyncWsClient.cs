using System.Collections.Concurrent;
using RaceDirector.Remote.Networking.Codec;

namespace RaceDirector.Remote.Networking.Client;

public class SyncWsClient<TOut, TIn> : WsClient<TOut, TIn>
{
    private readonly BlockingCollection<TIn> _received;
    private readonly TimeSpan _timeout;

    public SyncWsClient(string uri, ICodec<TOut, TIn> codec, TimeSpan timeout) : this(new Uri(uri), codec, timeout) { }

    public SyncWsClient(Uri uri, ICodec<TOut, TIn> codec, TimeSpan timeout) : base(uri, codec)
    {
        _received = new BlockingCollection<TIn>();
        _timeout = timeout;
    }

    public bool ConnectAndWait()
    {
        return ConnectAsync() && Connected.Wait(_timeout);
    }

    public TIn Next()
    {
        if (!_received.TryTake(out var body, _timeout))
            throw new TimeoutException("Message not received within timeout");
        return body;
    }

    protected override void OnWsReceived(TIn response)
    {
        _received.Add(response);
    }
}