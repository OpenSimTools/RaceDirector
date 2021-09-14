using AutoBogus;
using AutoBogus.Moq;
using HUD.Tests.Base;
using HUD.Tests.TestUtils;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Plugin.HUD.Pipeline;
using System.Net;
using Xunit;

namespace HUD.Tests.Pipeline
{
    public class DashboardServerTest : IntegrationTestBase
    {
        [Fact]
        public void ServesR3ETelemetryEndpoint()
        {
            using (var server = new DashboardServer(new DashboardServer.Config(IPAddress.Any, _serverPort)))
            {
                server.Start();
                using (var client = new JsonWsClient(Timeout, _serverPort, "/r3e"))
                {
                    var telemetry = AutoFaker.Generate<GameTelemetry>(b => b.WithBinder<MoqBinder>());

                    Assert.True(client.ConnectAndWait());
                    server.Multicast(telemetry);
                    var message = client.nextJson();
                    Assert.Equal(System.Text.Json.JsonValueKind.Number, message.Path("VersionMajor").ValueKind);
                    Assert.Equal(System.Text.Json.JsonValueKind.Number, message.Path("VersionMinor").ValueKind);
                }
            }
        }

        #region Test setup

        int _serverPort = Tcp.FreePort();

        #endregion
    }
}
