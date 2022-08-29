namespace RaceDirector.Remote.Networking.Server;

public interface ISession<TOut>
{
    object Id { get; }

    bool WsSendAsync(TOut t);
}