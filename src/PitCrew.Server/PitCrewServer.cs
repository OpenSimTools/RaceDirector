using System.Net;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using RaceDirector.Remote.Networking.Codec;
using RaceDirector.Remote.Networking.Server;

namespace PitCrew.Server;

public class PitCrewServer : MultiEndpointWsServer<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    private static readonly IEndpoint<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>[] _endpoint =
    {
        new Endpoint<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(_ => true, new IdentityCodec())
    };

    public PitCrewServer(Config config, ILogger<PitCrewServer> logger) : base(IPAddress.Any, config.Port, _endpoint, logger) {}

    protected override void OnWsReceived(WsSession session, ReadOnlyMemory<byte> message)
    {
        WsMulticastAsync(message, otherSession => otherSession.Id != session.Id);
    }
}