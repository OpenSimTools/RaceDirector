namespace RaceDirector.Remote.Networking.Client;

public interface IWsClient<in TOut>
{
    bool Connect();
    bool Disconnect();

    bool WsSendAsync(TOut t);
}