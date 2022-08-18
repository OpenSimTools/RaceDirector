namespace RaceDirector.Remote.Networking.Client;

public interface IWsClient<in TOut, out TIn> : IDisposable
{
    bool Connect();

    bool Disconnect();

    event MessageHandler<TIn> MessageHandler;
    
    void WsSendAsync(TOut t);
}