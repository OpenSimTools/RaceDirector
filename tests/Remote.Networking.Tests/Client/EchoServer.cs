using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Server;
using TestUtils;

namespace Remote.Networking.Tests.Client;

public class EchoServer : MultiEndpointWsServer<string, string>
{
    private static readonly HttpEndpoint<string, string>[] Endpoint =
    {
        new(_ => true, Codec.UTF8String)
    };

    public EchoServer() :
        base(IPAddress.Any, Tcp.FreePort(), Endpoint, NullLogger.Instance)
    {
        // The server would refuse to send an empty message, so we need to make
        // sure that empty messages from the client are still sent back.
        MessageHandler += (session, message) => session.WsSendAsync($"<- {message}");
    }
}