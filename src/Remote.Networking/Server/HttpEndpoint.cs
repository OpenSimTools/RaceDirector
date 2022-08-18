namespace RaceDirector.Remote.Networking.Server;

public record HttpEndpoint<TOut, TIn>(Func<IHttpRequest, bool> Matcher, Codec<TOut, TIn> Codec);

public static class HttpEndpoint
{
    public static Func<IHttpRequest, bool> PathMatcher(string path) =>
        request => path == request.Uri.AbsolutePath;
}