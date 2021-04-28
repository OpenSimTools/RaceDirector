using RaceDirector.Pipeline;
using System;
using System.Runtime.Versioning;

namespace RaceDirector.Main
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting pipeline");
            new PipelineRunner().Run().Wait();
        }
    }
}
