using System;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    public interface ITcpServer : IDisposable
    {
        bool Start();
        bool Stop();
    }
}
