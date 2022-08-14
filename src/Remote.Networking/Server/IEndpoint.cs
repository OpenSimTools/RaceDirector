using NetCoreServer;
using RaceDirector.Remote.Networking.Codec;

namespace RaceDirector.Remote.Networking.Server;

public interface IEndpoint<in TOut, out TIn>
{
    ICodec<TOut, TIn> Codec { get; }

    bool Matches(HttpRequest request);
}