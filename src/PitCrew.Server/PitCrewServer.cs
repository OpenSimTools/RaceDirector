using System.Net;
using Microsoft.Extensions.Logging;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Server;

namespace PitCrew.Server;

public class PitCrewServer : MultiEndpointWsServer<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    private static readonly HttpEndpoint<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>[] Endpoint =
    {
        new(_ => true, Codec.Identity)
    };

    public PitCrewServer(Config config, ILogger<PitCrewServer> logger) :
        base(IPAddress.Any, config.Port, Endpoint, logger)
    {
        MessageHandler += (session, message) => WsMulticastAsync(message, otherSession => otherSession.Id != session.Id);
    }
}