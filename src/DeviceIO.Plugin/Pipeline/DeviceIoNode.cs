using System.Reactive;
using Microsoft.Extensions.Logging;
using RaceDirector.Pipeline;

namespace RaceDirector.DeviceIO.Pipeline;

public class DeviceIoNode : INode
{
    public interface Configuration
    {
        public Dictionary<string, string> KeyMappings { get; }
    }
    
    public IObserver<GameAction> GameActionObserver { get; }

    public DeviceIoNode(Configuration configuration, ILogger<DeviceIoNode> logger)
    {
        GameActionObserver = Observer.Create<GameAction>(ga =>
        {
            if (configuration.KeyMappings.TryGetValue(ga.ToString(), out var keys))
            {
                KeyPresser.SendKeys(keys);
                logger.LogTrace("Received {GameAction} sent {Keys}", ga, keys);
            }
            else
            {
                logger.LogWarning("Received {GameAction} but no mapping found", ga);
            }
        });
    }
}