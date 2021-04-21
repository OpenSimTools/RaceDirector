using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Utils
{
    public static class PollingSource
    {
        public static ISourceBlock<T> Create<T>(TimeSpan pollingInterval, Func<T> f)
        {
            var source = new BufferBlock<T>();
            var processPoller = new Timer(
                callback: _ => source.Post(f()),
                state: null,
                dueTime: TimeSpan.Zero,
                period: pollingInterval
            );
            source.Completion.ContinueWith(task => processPoller.Dispose());
            return source;
        }
    }
}
