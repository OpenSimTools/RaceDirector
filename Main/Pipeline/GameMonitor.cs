using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline
{
    public class GameMonitor
    {
        public ISourceBlock<RunningGame> RunningGameSource
        {
            get;
        }

        public GameMonitor()
        {
            var bb = new BufferBlock<RunningGame>();
            bb.Post(new RunningGame(null));

            RunningGameSource = bb;
        }
    }
}
