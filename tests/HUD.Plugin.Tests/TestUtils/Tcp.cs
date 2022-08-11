using System.Net;
using System.Net.Sockets;

namespace HUD.Tests.TestUtils;

internal static class Tcp
{
    public static int FreePort()
    {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}