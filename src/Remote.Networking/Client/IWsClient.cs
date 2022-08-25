namespace RaceDirector.Remote.Networking.Client;

public interface IWsClient<in TOut, out TIn> : IConnectableConsumer<TOut>, IProducer<TIn>
{
    event MessageHandler<TIn> MessageHandler;
    
    void WsSendAsync(TOut t);
}