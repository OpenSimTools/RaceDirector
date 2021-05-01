using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    using ILiveTelemetry = RaceDirector.Pipeline.Telemetry.V0.ILiveTelemetry;
    using IRunningGame = RaceDirector.Pipeline.GameMonitor.V0.IRunningGame;

    public class WebSocketTelemetryNode : IDisposable
    {
        public record Config(int port = 8070);

        private readonly DashboardServer server;

        public WebSocketTelemetryNode(Config config, IEnumerable<ITelemetryEndpoint> endpoints)
        {
            server = new DashboardServer(config.port, endpoints);
            RunningGameTarget = new ActionBlock<IRunningGame>(runningGame => {
                if (runningGame.Name is null)
                    server.Stop();
                else
                    server.Start();
            });
            LiveTelemetryTarget = new ActionBlock<ILiveTelemetry>(_ => { });
        }

        public ITargetBlock<IRunningGame> RunningGameTarget
        {
            get;
        }

        public ITargetBlock<ILiveTelemetry> LiveTelemetryTarget
        {
            get;
        }

        public void Dispose()
        {
            RunningGameTarget.Complete();
            LiveTelemetryTarget.Complete();
            server.Dispose();
        }

        private class DashboardServer : WsServer
        {
            public DashboardServer(int port, IEnumerable<ITelemetryEndpoint> endpoints)
                : base(IPAddress.Any, port)
            {
            }
        }
    }
}
