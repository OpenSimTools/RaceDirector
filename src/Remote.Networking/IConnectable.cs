namespace RaceDirector.Remote;

public interface IConnectable : IDisposable
{
    bool Connect();
    bool Disconnect();
}