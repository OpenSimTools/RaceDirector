namespace RaceDirector.Remote.Networking;

public interface IProducer<out T>
{
    IObservable<T> In { get; }
}