using System;

namespace RaceDirector.Pipeline.GameMonitor
{
    // TODO this is in the interface to make testing easier, but it might be abused
    public record RunningGame(string? Name) : V0.IRunningGame;
}
