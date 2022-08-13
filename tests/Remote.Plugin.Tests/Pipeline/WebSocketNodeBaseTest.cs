using Moq;
using RaceDirector.Remote.Networking.Server;
using RaceDirector.Remote.Pipeline;
using Xunit;
using Xunit.Categories;
using static TestUtils.EventuallyAssertion;

namespace Remote.Plugin.Tests.Pipeline;

[IntegrationTest]
public class WebSocketNodeBaseTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void ServerIsStartedWhenTriggered()
    {
        _webSocketNode.PostTrigger(true);
        foreach (var sm in _serverMocks)
            Eventually(() => sm.Verify(s => s.Start()))
                .OrError("Server wasn't started")
                .Within(Timeout);
    }

    [Fact]
    public void ServerIsStoppedWhenTriggered()
    {
        _webSocketNode.PostTrigger(false);
        foreach (var sm in _serverMocks)
            Eventually(() => sm.Verify(s => s.Stop()))
                .OrError("Server didn't stop")
                .Within(Timeout);
    }

    [Fact]
    public void DataIsBroadcastedWhenReceived()
    {
        _webSocketNode.PostData(2);
        foreach (var sm in _serverMocks)
            Eventually(() => sm.Verify(s => s.WsMulticastAsync(2)))
                .OrError("Data wasn't broadcasted")
                .Within(Timeout);
    }

    #region Test setup

    private readonly IEnumerable<Mock<IWsServer<int>>> _serverMocks;
    private readonly TestWebSocketNode _webSocketNode;

    public WebSocketNodeBaseTest()
    {
        _serverMocks = Enumerable.Range(1, 3).Select(_ => new Mock<IWsServer<int>>()).ToArray();
        var mockedServers = _serverMocks.Select(m => m.Object).ToArray();
        _webSocketNode = new TestWebSocketNode(mockedServers);
    }

    private class TestWebSocketNode : WebSocketNodeBase<bool, int>
    {
        public TestWebSocketNode(params IWsServer<int>[] servers) : base(servers) { }

        protected override bool ServerShouldRun(bool trigger)
        {
            return trigger;
        }

        public void PostTrigger(bool trigger)
        {
            TriggerObserver.OnNext(trigger);
        }

        public void PostData(int data)
        {
            DataObserver.OnNext(data);
        }
    }

    #endregion
}