using System.Net;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using RaceDirector.Remote.Networking.Codec;
using RaceDirector.Remote.Networking.Server;

namespace PitCrew.Server;

public class PitCrewServer : MultiEndpointWsServer<string, string>
{
    private static readonly IEndpoint<string, string>[] _endpoint =
    {
        new Endpoint<string, string>(_ => true, StringCodec.UTF8)
    };

    public PitCrewServer(Config config, ILogger<PitCrewServer> logger) : base(IPAddress.Any, config.Port, _endpoint, logger) {}

    protected override void OnWsReceived(WsSession session, string message)
    {
        WsMulticastAsync(message, otherSession => otherSession.Id != session.Id);
    }
}