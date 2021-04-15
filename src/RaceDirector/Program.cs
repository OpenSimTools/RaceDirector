using RaceDirector.Pipeline.SimMonitor;
using System;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new SimMonitorNode.Config(new[] { "notepad" }, TimeSpan.FromSeconds(1));
            using (var simMonitorNode = new SimMonitorNode(config))
            {
                var source = simMonitorNode.RunningSimSource;
                var simNameLogger = new ActionBlock<RunningSim>(runningSim => Console.WriteLine("====================> " + runningSim.Name));
                source.LinkTo(simNameLogger);
                source.Completion.Wait();
            }
        }
    }
}
