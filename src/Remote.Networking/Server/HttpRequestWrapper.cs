using NetCoreServer;

namespace RaceDirector.Remote.Networking.Server;

internal struct HttpRequestWrapper : IHttpRequest
{
    private readonly HttpRequest _inner;

    public Uri Uri { get; }

    public HttpRequestWrapper(Uri baseUri, HttpRequest inner)
    {
        _inner = inner;
        Uri = new Uri(baseUri, inner.Url);
    }
}