using System;

namespace RaceDirector.Plugin.HUD.Server;

public interface ITcpServer : IDisposable
{
    bool Start();
    bool Stop();
}