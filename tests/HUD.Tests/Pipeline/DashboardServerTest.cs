using AutoBogus;
using AutoBogus.Moq;
using HUD.Tests.Base;
using HUD.Tests.TestUtils;
using RaceDirector.Pipeline.Telemetry.V0;
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
                    var telemetry = AutoFaker.Generate<IGameTelemetry>(b => b.WithBinder<MoqBinder>());

                    Assert.True(client.ConnectAndWait());
                    server.Multicast(telemetry);
                    var message = client.nextJson();
                    Assert.Equal(2, message.Path("VersionMajor").GetInt32());
                    Assert.Equal(10, message.Path("VersionMinor").GetInt32());
                }
            }
        }

        #region Test setup

        int _serverPort = Tcp.FreePort();

        #endregion
    }
}
