using System.Net;
using Microsoft.Extensions.Logging;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Server;

namespace PitCrew.Server;

public class PitCrewServer : MultiEndpointWsServer<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    protected override bool UseDefaults => true;

    private static readonly HttpEndpoint<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>[] Endpoint =
    {
        new(_ => true, Codec.Identity)
    };

    public PitCrewServer(Config config, ILogger<PitCrewServer> logger) :
        base(IPAddress.Any, config.Port, Endpoint, logger)
    {
        AddStaticContent("ui");
        MessageHandler += (session, message) => WsMulticastAsync(message, _ => !session.Id.Equals(_.Id));
    }
}