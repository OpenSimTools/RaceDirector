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
                    Assert.Equal(System.Text.Json.JsonValueKind.Number, message.Path("VersionMajor").ValueKind);
                    Assert.Equal(System.Text.Json.JsonValueKind.Number, message.Path("VersionMinor").ValueKind);
                }
            }
        }

        [Fact]
        public void Working()
        {
            var telemetry =
                new AutoFaker<RaceDirector.Pipeline.Telemetry.GameTelemetry>()
                .Configure(b => b.WithBinder<MoqBinder>())
                .RuleFor(t => t.Event, f =>
                    new AutoFaker<RaceDirector.Pipeline.Telemetry.Event>()
                        .Configure(b => b.WithBinder<MoqBinder>())
                        .RuleFor(e => e.FuelRate, f => 2.0)
                        .Generate()
                    )
                .Generate();

            Assert.Equal(2.0, telemetry.Event?.FuelRate);
            Assert.NotNull(telemetry.Session);
        }

        [Fact]
        public void NotWorking()
        {
            var telemetry =
                new AutoFaker<IGameTelemetry>()
                .Configure(b => b.WithBinder<MoqBinder>())
                .RuleFor(t => t.Event, f =>
                    new AutoFaker<IEvent>()
                        .Configure(b => b.WithBinder<MoqBinder>())
                        .RuleFor(e => e.FuelRate, f => 2.0)
                        .Generate()
                    )
                .Generate();

            Assert.Equal(2.0, telemetry.Event?.FuelRate);
            Assert.NotNull(telemetry.Session);
        }

        #region Test setup

        int _serverPort = Tcp.FreePort();

        #endregion
    }
}
