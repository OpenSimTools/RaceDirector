using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDirector.Plugin.HUD.Server
{
    public interface IWsServer<T> : ITcpServer
    {
        bool Multicast(T t);
    }
}
