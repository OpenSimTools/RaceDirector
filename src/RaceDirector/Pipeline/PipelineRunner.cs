using RaceDirector.Pipeline.SimMonitor;
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
            var runningSimSource = SimMonitorNode();
            var simNameLogger = RunningSimLogger();
            runningSimSource.LinkTo(simNameLogger);
            return runningSimSource.Completion;
        }

        private ISourceBlock<RunningSim> SimMonitorNode()
        {
            var config = new SimMonitorNode.Config(new[] { "RRRE64" }, TimeSpan.FromSeconds(5));
            var simMonitorNode = new SimMonitorNode(config);
            return simMonitorNode.RunningSimSource;
        }

        private ITargetBlock<RunningSim> RunningSimLogger()
        {
            return new ActionBlock<RunningSim>(runningSim => Console.WriteLine("> " + runningSim.Name));
        }
    }
}
