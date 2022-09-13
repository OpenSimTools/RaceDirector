using Moq;
using RaceDirector.Remote;
using RaceDirector.Remote.Pipeline;
using Xunit;
using Xunit.Categories;
using static TestUtils.EventuallyAssertion;

namespace Remote.Plugin.Tests.Pipeline;

[IntegrationTest]
public class WsNodeBaseTest
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void ServerIsStartedWhenTriggered()
    {
        _wsNode.PostTrigger(true);
        foreach (var sm in _pusherMocks)
            Eventually(() => sm.Verify(s => s.Start()))
                .OrError("Server wasn't started")
                .Within(Timeout);
    }

    [Fact]
    public void ServerIsStoppedWhenTriggered()
    {
        _wsNode.PostTrigger(false);
        foreach (var sm in _pusherMocks)
            Eventually(() => sm.Verify(s => s.Stop()))
                .OrError("Server didn't stop")
                .Within(Timeout);
    }

    [Fact]
    public void DataIsBroadcastedWhenReceived()
    {
        _wsNode.PostData(2);
        foreach (var sm in _pusherMocks)
            Eventually(() => sm.Verify(s => s.PushAsync(2)))
                .OrError("Data wasn't broadcasted")
                .Within(Timeout);
    }

    #region Test setup

    private readonly IEnumerable<Mock<IRemotePusher<int>>> _pusherMocks;
    private readonly TestWsNode _wsNode;

    public WsNodeBaseTest()
    {
        _pusherMocks = Enumerable.Range(1, 3).Select(_ => new Mock<IRemotePusher<int>>()).ToArray();
        var mockedServers = _pusherMocks.Select(m => m.Object).ToArray();
        _wsNode = new TestWsNode(mockedServers);
    }

    private class TestWsNode : WsNodeBase<bool, int>
    {
        public TestWsNode(params IRemotePusher<int>[] pushers) : base(pushers) { }

        protected override bool PusherShouldStart(bool trigger)
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