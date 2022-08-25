namespace RaceDirector.Remote.Networking;

public interface IConsumer<in T>
{
    IObserver<T> Out { get; }
}