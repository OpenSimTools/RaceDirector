using NetCoreServer;

namespace RaceDirector.Remote.Networking.Server;

public interface IEndpoint<T>
{
    bool Matches(HttpRequest request);

    byte[] Transform(T t);
}