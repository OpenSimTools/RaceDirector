using NetCoreServer;
using System;

namespace RaceDirector.HUD.Server;

public class Endpoint<T> : IEndpoint<T>
{
    private readonly Func<HttpRequest, bool> _matches;
    private readonly Func<T, byte[]> _transform;

    public Endpoint(Func<HttpRequest, bool> matches, Func<T, byte[]> transform)
    {
        _matches = matches;
        _transform = transform;
    }

    public bool Matches(HttpRequest request) => _matches(request);

    public byte[] Transform(T t) => _transform(t);
}

public static class Endpoint
{
    public static Func<HttpRequest, bool> PathMatcher(string path) => request =>
    {
        var requestPath = request.Url.Split('?', 1)[0];
        return path == requestPath;
    };
}