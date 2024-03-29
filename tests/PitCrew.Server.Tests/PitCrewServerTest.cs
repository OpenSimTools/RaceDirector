﻿using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDirector.Remote.Networking;
using RaceDirector.Remote.Networking.Client;
using TestUtils;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Server.Tests;

[IntegrationTest]
public class PitCrewServerTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void BroadcastsToOtherConnectedClients()
    {
        WithServer(serverPort =>
        {
            using var client1 = ConnectedClient(serverPort);
            using var client2 = ConnectedClient(serverPort);
            client1.WsSendAsync("c1");
            client2.WsSendAsync("c2");
            Assert.Equal("c1", client2.Next());
            Assert.Equal("c2", client1.Next());
        });
    }

    [Fact]
    public void ServesUI()
    {
        WithServer(serverPort =>
        {
            using var client = new HttpClient();
            var task = client.GetStringAsync($"http://{IPAddress.Loopback}:{serverPort}/ui/index.js");
            Assert.Contains("function", task.Result);
        });
    }

    [Fact]
    public void UIDefaultsToIndexHtmlForDirectories()
    {
        WithServer(new Config { ServeUI = true }, serverPort =>
        {
            using var client = new HttpClient();
            var task = client.GetStringAsync($"http://{IPAddress.Loopback}:{serverPort}/ui/");
            Assert.StartsWith("<!doctype html>", task.Result);
        });
    }

    [Fact]
    public void UIServingCanBeDisabled()
    {
        WithServer(new Config { ServeUI = false },serverPort =>
        {
            using var client = new HttpClient();
            var task = client.GetStringAsync($"http://{IPAddress.Loopback}:{serverPort}/ui/");
            // Very sad...
            Assert.False(task.Wait(Timeout.Milliseconds), "It returned something!");
        });
    }

    private static void WithServer(Action<int> action) => WithServer(new Config(), action);

    private static void WithServer(Config config, Action<int> action)
    {
        var serverPort = Tcp.FreePort();
        config.Port = serverPort;
        using var server = new PitCrewServer(config, NullLogger<PitCrewServer>.Instance);
        Assert.True(server.Start());
        action(serverPort);
    }

    private static SyncWsClient<string, string> ConnectedClient(int serverPort)
    {
        var client = new SyncWsClient<string, string>($"ws://{IPAddress.Loopback}:{serverPort}", Codec.UTF8String, Timeout);
        Assert.True(client.ConnectAndWait());
        return client;
    }
}