using HUD.Tests.Base;
using HUD.Tests.TestUtils;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Plugin.HUD.Pipeline;
using RaceDirector.Plugin.HUD.Pipeline.Config;
using System;
using System.Net;
using Xunit;

namespace HUD.Tests.Pipeline
{
    public class DashboardServerTest : IntegrationTestBase
    {
        [Fact]
        public void ServesR3ETelemetryEndpoint()
        {
            using (var server = new DashboardServer(new DashboardServerConfig(IPAddress.Any, _serverPort)))
            {
                server.Start();
                using (var client = new JsonWsClient(Timeout, _serverPort, "/r3e"))
                {
                    Assert.True(client.ConnectAndWait());
                    server.Multicast(new LiveTelemetry(TimeSpan.FromSeconds(4.2)));
                    var message = client.nextJson();
                    Assert.Equal(4.2, message.Path("Player", "GameSimulationTime").GetDouble());
                }
            }
        }

        #region Test setup

        int _serverPort = Tcp.FreePort();

        #endregion
    }
}
