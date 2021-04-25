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
                callback: _ =>
                {
                    try
                    {
                        source.Post(f());
                    }
                    catch (Exception e) {
                        // System.Threading.Timer does not swallow exceptions. Unfortunately,
                        // tests don't catch this but the debugger halts otherwise.
                    }
                },
                state: null,
                dueTime: TimeSpan.Zero,
                period: pollingInterval
            );
            source.Completion.ContinueWith(task => processPoller.Dispose());
            return source;
        }
    }
}
