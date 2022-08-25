namespace RaceDirector.Remote;

public interface IStartable : IDisposable
{
    bool Start();
    bool Stop();
}