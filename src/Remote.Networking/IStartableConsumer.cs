namespace RaceDirector.Remote.Networking;

public interface IStartableConsumer<in T> : IConsumer<T>, IStartable
{
}