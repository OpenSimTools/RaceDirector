﻿using System;

namespace RaceDirector.Pipeline.GameMonitor
{
    public record RunningGame(string? Name) : V0.IRunningGame;
}