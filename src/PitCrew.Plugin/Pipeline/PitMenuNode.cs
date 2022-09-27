﻿using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using RaceDirector.DeviceIO.Pipeline;
using RaceDirector.Pipeline;
using RaceDirector.Pipeline.GameMonitor.V0;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.PitCrew.Pipeline.Games;
using RaceDirector.PitCrew.Protocol;
using RaceDirector.Reactive;

namespace RaceDirector.PitCrew.Pipeline;

public class PitMenuNode : INode
{
    public IObserver<IPitStrategyRequest> PitStrategyObserver { get; }

    public IObserver<IRunningGame> RunningGameObserver { get; }

    public IObserver<IGameTelemetry> GameTelemetryObserver { get; }

    public IObservable<GameAction> GameActionObservable { get; }

    public PitMenuNode(IEnumerable<IGamePitMenuNavigator> pitMenuNavigators, ILogger<PitMenuNode> logger)
    {
        var pitStrategySubject = new Subject<IPitStrategyRequest>();
        var gameTelemetrySubject = new ReplaySubject<IGameTelemetry>(1);
        var runningGameSubject = new ReplaySubject<IRunningGame>(1);

        PitStrategyObserver = pitStrategySubject;
        RunningGameObserver = runningGameSubject;
        GameTelemetryObserver = gameTelemetrySubject;
        GameActionObservable = pitStrategySubject
            .SelectManyConcat(pitStrategy =>
                PitMenuNavigator(runningGameSubject, pitMenuNavigators).SelectMany(navigator =>
                    navigator.ApplyStrategy(pitStrategy, gameTelemetrySubject, logger)
                )
            );
    }
    
    private IObservable<IGamePitMenuNavigator> PitMenuNavigator(IObservable<IRunningGame> runningGameObservable,
        IEnumerable<IGamePitMenuNavigator> pitMenuNavigators) =>
        runningGameObservable.Select(runningGame =>
            pitMenuNavigators.Where(_ => _.GameName.Equals(runningGame.Name)).SingleOrDefault(new NullPitMenuNavigator()));
    
    private class NullPitMenuNavigator : IGamePitMenuNavigator
    {
        public string GameName => throw new NotSupportedException();

        public IObservable<GameAction> ApplyStrategy(IPitStrategyRequest request, IObservable<IGameTelemetry> gto, ILogger logger)
        {
            logger.LogWarning("This game does not support setting pit strategies");
            return Observable.Empty<GameAction>();
        }
    }
}