using NetCoreServer;

namespace RaceDirector.HUD.Server;

public interface IEndpoint<T>
{
    bool Matches(HttpRequest request);

    byte[] Transform(T t);
}