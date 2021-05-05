using HUD.Tests.Base;
using Moq;
using RaceDirector.Plugin.HUD.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
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
                Eventually(() => sm.Verify(s => s.Start()));
        }

        [Fact]
        public void ServerIsStoppedWhenTriggered()
        {
            _webSocketNode.PostTrigger(false);
            foreach (var sm in _serverMocks)
                Eventually(() => sm.Verify(s => s.Stop()));
        }

        [Fact]
        public void DataIsBroadcastedWhenReceived()
        {
            _webSocketNode.PostData(2);
            foreach (var sm in _serverMocks)
                Eventually(() => sm.Verify(s => s.Multicast(2)));
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

            protected override bool ShouldRun(bool trigger)
            {
                return trigger;
            }

            public void PostTrigger(bool trigger)
            {
                TriggerTarget.Post(trigger);
            }

            public void PostData(int data)
            {
                DataTarget.Post(data);
            }
        }

        #endregion
    }
}
