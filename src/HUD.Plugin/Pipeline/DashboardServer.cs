using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Remote.Networking.Server;
using static RaceDirector.Remote.Networking.Server.Endpoint;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Codec;

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

    private static IEnumerable<IEndpoint<IGameTelemetry, Nothing>> DashboardEndpoints(Config config)
    {
        var r3EDashTransformer = new R3EDashEncoder(config.R3EDash);
        return new[]
        {
            new Endpoint<IGameTelemetry, Nothing>(PathMatcher("/r3e"), r3EDashTransformer.ToCodec())
        };
    }

    public DashboardServer(Config config, ILogger<DashboardServer> logger) :
        base(config.Address, config.Port, DashboardEndpoints(config), logger)
    {
    }

    protected override void OnStarted()
    {
        Logger.LogInformation("Dashboard server started");
    }

    protected override void OnStopped()
    {
        Logger.LogInformation("Dashboard server stopped");
    }
}