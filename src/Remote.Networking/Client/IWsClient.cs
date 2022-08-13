namespace RaceDirector.Remote.Networking.Client;

public interface IWsClient<in TOut>
{
    bool Connect();
    bool Disconnect();

    bool WeSendAsync(TOut t);
}