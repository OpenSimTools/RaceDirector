using NetCoreServer;
using System;
using RaceDirector.Remote.Networking.Codec;

namespace RaceDirector.Remote.Networking.Server;

public class Endpoint<TOut, TIn> : IEndpoint<TOut, TIn>
{
    private readonly Func<HttpRequest, bool> _matches;
    public ICodec<TOut, TIn> Codec { get; }

    public Endpoint(Func<HttpRequest, bool> matches, ICodec<TOut, TIn> codec)
    {
        _matches = matches;
        Codec = codec;
    }

    public bool Matches(HttpRequest request) => _matches(request);
}

public static class Endpoint
{
    public static Func<HttpRequest, bool> PathMatcher(string path) => request =>
    {
        var requestPath = request.Url.Split('?', 1)[0];
        return path == requestPath;
    };
}