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
            var keys = configuration.KeyMappings[ga.ToString()];
            KeyPresser.SendKeys(keys);
            logger.LogTrace("Received {GameAction} sent {Keys}", ga, keys);
        });
    }
}