using Moq;
using RaceDirector.Remote;
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
        foreach (var sm in _publisherMocks)
            Eventually(() => sm.Verify(s => s.Start()))
                .OrError("Server wasn't started")
                .Within(Timeout);
    }

    [Fact]
    public void ServerIsStoppedWhenTriggered()
    {
        _webSocketNode.PostTrigger(false);
        foreach (var sm in _publisherMocks)
            Eventually(() => sm.Verify(s => s.Stop()))
                .OrError("Server didn't stop")
                .Within(Timeout);
    }

    [Fact]
    public void DataIsBroadcastedWhenReceived()
    {
        _webSocketNode.PostData(2);
        foreach (var sm in _publisherMocks)
            Eventually(() => sm.Verify(s => s.PublishAsync(2)))
                .OrError("Data wasn't broadcasted")
                .Within(Timeout);
    }

    #region Test setup

    private readonly IEnumerable<Mock<IRemotePublisher<int>>> _publisherMocks;
    private readonly TestWebSocketNode _webSocketNode;

    public WebSocketNodeBaseTest()
    {
        _publisherMocks = Enumerable.Range(1, 3).Select(_ => new Mock<IRemotePublisher<int>>()).ToArray();
        var mockedServers = _publisherMocks.Select(m => m.Object).ToArray();
        _webSocketNode = new TestWebSocketNode(mockedServers);
    }

    private class TestWebSocketNode : WebSocketNodeBase<bool, int>
    {
        public TestWebSocketNode(params IRemotePublisher<int>[] publishers) : base(publishers) { }

        protected override bool PublisherShouldStart(bool trigger)
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