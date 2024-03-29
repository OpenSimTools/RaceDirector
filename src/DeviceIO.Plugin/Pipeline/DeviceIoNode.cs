﻿using System.Reactive;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using RaceDirector.Pipeline;
using RaceDirector.Reactive;

namespace RaceDirector.DeviceIO.Pipeline;

public class DeviceIoNode : INode
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeviceIoNode> _logger;

    public interface IConfiguration
    {
        Dictionary<string, string> KeyMappings { get; }
        TimeSpan WaitBetweenKeys { get; }
    }
    
    public IObserver<GameAction> GameActionObserver { get; }

    public DeviceIoNode(IConfiguration configuration, ILogger<DeviceIoNode> logger)
    {
        _configuration = configuration;
        _logger = logger;
        var subject = new Subject<GameAction>();
        subject
            .SpacedBy(_configuration.WaitBetweenKeys)
            .Subscribe(Observer.Create<GameAction>(SendKeysToGame));
        GameActionObserver = subject;
    }

    private void SendKeysToGame(GameAction ga)
    {
        if (_configuration.KeyMappings.TryGetValue(ga.ToString(), out var keys))
        {
            SendKeys.SendWait(keys);
            _logger.LogTrace("Received {GameAction} sent {Keys}", ga, keys);
        }
        else
        {
            _logger.LogWarning("Received {GameAction} but no mapping found", ga);
        }
    }
}