using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Remote.Networking.Server;
using static RaceDirector.Remote.Networking.Server.HttpEndpoint;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using RaceDirector.Remote.Networking;

namespace RaceDirector.HUD.Pipeline;

/// <summary>
/// Exposes live telemetry for dashboards.
/// </summary>
public class DashboardServer : MultiEndpointWsServer<IGameTelemetry, Nothing>
{
    public class Config
    {
        public IPAddress Address { get; set; } = IPAddress.Any;
        public int Port { get; set; } = 8070;

        public R3EDashEncoder.Configuration R3EDash { get; set; } = new();
    }

    private static IEnumerable<HttpEndpoint<IGameTelemetry, Nothing>> DashboardEndpoints(Config config)
    {
        var r3EDashTransformer = new R3EDashEncoder(config.R3EDash);
        return new HttpEndpoint<IGameTelemetry, Nothing>[]
        {
            new(PathMatcher("/r3e"), Codec.EncodeOnly<IGameTelemetry>(r3EDashTransformer.Encode))
        };
    }

    public DashboardServer(Config config, ILogger<DashboardServer> logger) :
        base(config.Address, config.Port, DashboardEndpoints(config), logger)
    {
    }
}