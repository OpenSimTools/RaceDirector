namespace RaceDirector.Remote.Networking;

public interface IConnectableConsumer<in T> : IConsumer<T>, IConnectable
{
}