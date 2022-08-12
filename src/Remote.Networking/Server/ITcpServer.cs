using System;

namespace RaceDirector.Remote.Networking.Server;

public interface ITcpServer : IDisposable
{
    bool Start();
    bool Stop();
}