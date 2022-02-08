using HUD.Tests.Base;
using Moq;
using RaceDirector.Plugin.HUD.Pipeline;
using RaceDirector.Plugin.HUD.Server;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HUD.Tests.Pipeline
{
    public class WebSocketNodeBaseTest : IntegrationTestBase
    {
        [Fact]
        public void ServerIsStartedWhenTriggered()
        {
            _webSocketNode.PostTrigger(true);
            foreach (var sm in _serverMocks)
                Eventually(() => sm.Verify(s => s.Start()), "Server wasn't started");
        }

        [Fact]
        public void ServerIsStoppedWhenTriggered()
        {
            _webSocketNode.PostTrigger(false);
            foreach (var sm in _serverMocks)
                Eventually(() => sm.Verify(s => s.Stop(), "Server didn't stop"));
        }

        [Fact]
        public void DataIsBroadcastedWhenReceived()
        {
            _webSocketNode.PostData(2);
            foreach (var sm in _serverMocks)
                Eventually(() => sm.Verify(s => s.Multicast(2)), "Data wasn't broadcasted");
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
                TriggerTarget.OnNext(trigger);
            }

            public void PostData(int data)
            {
                DataTarget.OnNext(data);
            }
        }

        #endregion
    }
}
