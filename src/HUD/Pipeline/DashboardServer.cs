using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Plugin.HUD.Server;
using static RaceDirector.Plugin.HUD.Server.Endpoint;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Net;
using System;
using RaceDirector.Plugin.HUD.Utils;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Exposes live telemetry for dashboards.
    /// </summary>
    public class DashboardServer : MultiEndpointWsServer<IGameTelemetry>
    {
        public record Config(IPAddress address, int port = 8070); // TODO remove when config done

        private static readonly IEnumerable<IEndpoint<IGameTelemetry>> _endpoints = new[] {
            new Endpoint<IGameTelemetry>(PathMatcher("/r3e"), R3EDashTransformer.ToR3EDash)
        };

        public DashboardServer(Config config) : base(config.address, config.port, _endpoints) { }
    }
}
