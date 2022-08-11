using System;

namespace RaceDirector.HUD.Server;

public interface ITcpServer : IDisposable
{
    bool Start();
    bool Stop();
}