using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Plugin.HUD.Server;
using static RaceDirector.Plugin.HUD.Server.Endpoint;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Net;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Exposes live telemetry for dashboards.
    /// </summary>
    public class DashboardServer : MultiEndpointWsServer<ILiveTelemetry>
    {
        public record Config(IPAddress address, int port = 8070); // TODO remove when config done

        private static readonly IEnumerable<IEndpoint<ILiveTelemetry>> _endpoints = new[] {
            new Endpoint<ILiveTelemetry>(PathMatcher("/r3e"), ToR3EDash)
        };

        private static readonly JsonWriterOptions jsonWriterOptions = new JsonWriterOptions();

        public DashboardServer(Config config) : base(config.address, config.port, _endpoints) { }

        private static byte[] ToR3EDash(ILiveTelemetry telemetry)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, jsonWriterOptions))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Player");
                        writer.WriteStartObject();
                        writer.WriteNumber("GameSimulationTime", telemetry.SimulationTime.TotalSeconds);
                        writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                return stream.ToArray();
            }
        }
    }
}
