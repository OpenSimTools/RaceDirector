using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Plugin.HUD.Server;
using static RaceDirector.Plugin.HUD.Server.Endpoint;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Exposes live telemetry for dashboards.
    /// </summary>
    public class DashboardServer : MultiEndpointWsServer<IGameTelemetry>
    {
        public class Config
        {
            public IPAddress Address { get; set; } = IPAddress.Any;
            public int Port { get; set; } = 8070;
        }

        private static readonly IEnumerable<IEndpoint<IGameTelemetry>> DashboardEndpoints = new[] {
            new Endpoint<IGameTelemetry>(PathMatcher("/r3e"), R3EDashTransformer.ToR3EDash)
        };

        public DashboardServer(Config config, ILogger<DashboardServer> logger) : base(config.Address, config.Port, DashboardEndpoints, logger) { }
        
        protected override void OnStarted()
        {
            Logger.LogInformation("Dashboard server started");
        }

        protected override void OnStopped()
        {
            Logger.LogInformation("Dashboard server stopped");
        }

    }
}
