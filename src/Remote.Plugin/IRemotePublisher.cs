using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Server;

namespace RaceDirector.Remote;

public interface IRemotePublisher<in T> : IStartable
{
    void PublishAsync(T t);
}

public static class WsServerEx
{
    public static IRemotePublisher<TOut> ToRemotePublisher<TOut, TIn>(this IWsServer<TOut, TIn> server)
        => new RemotePublisher<TOut, TIn> { Server = server };

    private readonly struct RemotePublisher<TOut, TIn> : IRemotePublisher<TOut>
    {
        internal IWsServer<TOut, TIn> Server { get; init; }

        public void PublishAsync(TOut t) => Server.WsMulticastAsync(t);

        public void Start() => Server.Start();

        public void Stop() => Server.Stop();
    }
}

public static class WsClientEx
{
    public static IRemotePublisher<TOut> ToRemotePublisher<TOut, TIn>(this IWsClient<TOut, TIn> client)
        => new RemotePublisher<TOut, TIn> { Client = client };

    private readonly struct RemotePublisher<TOut, TIn> : IRemotePublisher<TOut>
    {
        internal IWsClient<TOut, TIn> Client { get; init; }

        public void PublishAsync(TOut t) => Client.WsSendAsync(t);

        public void Start() => Client.Connect();

        public void Stop() => Client.Disconnect();
    }
}