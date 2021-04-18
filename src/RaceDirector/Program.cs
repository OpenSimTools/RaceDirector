using RaceDirector.Pipeline;
using System;

namespace RaceDirector.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting pipeline");
            new PipelineRunner().Run().Wait();
        }
    }
}
