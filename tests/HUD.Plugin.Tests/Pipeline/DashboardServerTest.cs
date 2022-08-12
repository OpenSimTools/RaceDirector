using System;
using AutoBogus;
using AutoBogus.Moq;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.HUD.Pipeline;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking.Client;
using RaceDirector.Remote.Networking.Utils;
using Xunit;
using Xunit.Categories;

namespace HUD.Plugin.Tests.Pipeline;

[IntegrationTest]
public class DashboardServerTest
{
    private static TimeSpan Timeout { get => TimeSpan.FromMilliseconds(500); }

    private static Bogus.Faker<GameTelemetry> gtFaker = new AutoFaker<GameTelemetry>()
        .Configure(b => b
            .WithBinder<MoqBinder>()
            // For some reason AutoBogus/Moq can't generate IDistance or IFraction<IDistance>
            .WithOverride(agoc => IDistance.FromM(agoc.Faker.Random.Int()))
            .WithOverride(agoc => DistanceFraction.Of(agoc.Generate<IDistance>(), agoc.Faker.Random.Double()))
        );

    [Fact]
    public void ServesR3ETelemetryEndpoint()
    {
        using var server = new DashboardServer(new DashboardServer.Config { Address = IPAddress.Any, Port = _serverPort }, TestLogger);
        Assert.True(server.Start(), "Server did not start");
        using var client = new JsonWsClient(Timeout, _serverPort, "/r3e");
        var telemetry = gtFaker.Generate();

        Assert.True(client.ConnectAndWait(), "Client could not connect");
        server.Multicast(telemetry);
        var message = client.NextJson();
        Assert.Equal(System.Text.Json.JsonValueKind.Number, message.Path("VersionMajor").ValueKind);
        Assert.Equal(System.Text.Json.JsonValueKind.Number, message.Path("VersionMinor").ValueKind);
    }

    #region Test setup

    private readonly int _serverPort = Tcp.FreePort();
    private static readonly ILogger<DashboardServer> TestLogger = NullLogger<DashboardServer>.Instance;

    #endregion
}