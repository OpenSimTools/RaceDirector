using RaceDirector.Pipeline.GameMonitor;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline
{

    public class PipelineRunner
    {
        /// <summary>
        /// Constructs and run the whole pipeline.
        /// </summary>
        public Task Run()
        {
            var runningGameSource = RunningGameSource();
            var runningGameLogger = RunningGameLogger();
            runningGameSource.LinkTo(runningGameLogger);
            return runningGameSource.Completion;
        }

        private ISourceBlock<RunningGame> RunningGameSource()
        {
            var config = new ProcessMonitorNode.Config(new[] { "RRRE64" }, TimeSpan.FromSeconds(5));
            var processMonitorNode = new ProcessMonitorNode(config);
            return processMonitorNode.RunningGameSource;
        }

        private ITargetBlock<RunningGame> RunningGameLogger()
        {
            return new ActionBlock<RunningGame>(runningGame => Console.WriteLine("> " + runningGame.Name));
        }
    }
}
