using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDirector.Remote.Networking.Server;

public interface IWsServer<in TOut> : ITcpServer
{
    bool WsMulticastAsync(TOut message);
}