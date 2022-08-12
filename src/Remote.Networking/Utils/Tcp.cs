using System.Net;
using System.Net.Sockets;

namespace RaceDirector.Remote.Networking.Utils;

public static class Tcp
{
    public static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}