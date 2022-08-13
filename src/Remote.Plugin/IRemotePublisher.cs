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
    public static IRemotePublisher<T> ToRemotePublisher<T>(this IWsServer<T> server)
        => new RemotePublisher<T>(server);

    private record RemotePublisher<T>(IWsServer<T> Server) : IRemotePublisher<T>
    {
        public void PublishAsync(T t) => Server.WsMulticastAsync(t);

        public void Start() => Server.Start();

        public void Stop() => Server.Stop();
    }
}

public static class WsClientEx
{
    public static IRemotePublisher<T> ToRemotePublisher<T>(this IWsClient<T> server)
        => new RemotePublisher<T>(server);

    private record RemotePublisher<T>(IWsClient<T> Client) : IRemotePublisher<T>
    {
        public void PublishAsync(T t) => Client.WeSendAsync(t);

        public void Start() => Client.Connect();

        public void Stop() => Client.Disconnect();
    }
}