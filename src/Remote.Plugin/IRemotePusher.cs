using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Server;

namespace RaceDirector.Remote;

public interface IRemotePusher<in T>
{
    void Start();
    void Stop();
    void PushAsync(T t);
}

public static class StartablePublisherEx
{
    public static IRemotePusher<T> ToRemotePusher<T>(this IStartableConsumer<T> consumer)
        => new RemotePusher<T>(consumer);

    private class RemotePusher<T> : IRemotePusher<T>
    {
        private readonly IStartableConsumer<T> _consumer;

        public RemotePusher(IStartableConsumer<T> consumer)
        {
            _consumer = consumer;
        }

        public void PushAsync(T t) => _consumer.Out.OnNext(t);

        public void Start() => _consumer.Start();

        public void Stop() => _consumer.Stop();
    }
}

public static class ConnectablePublisherEx
{
    public static IRemotePusher<T> ToRemotePusher<T>(this IConnectableConsumer<T> consumer)
        => new RemotePusher<T>(consumer);

    private class RemotePusher<T> : IRemotePusher<T>
    {
        private readonly IConnectableConsumer<T> _consumer;

        public RemotePusher(IConnectableConsumer<T> consumer)
        {
            _consumer = consumer;
        }

        public void PushAsync(T t) => _consumer.Out.OnNext(t);

        public void Start() => _consumer.Connect();

        public void Stop() => _consumer.Disconnect();
    }
}