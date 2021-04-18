using System;
using System.Diagnostics;
using System.Threading;

namespace RaceDirector.Tests.Contrib.Process1
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Write("Waiting... ");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Debug.WriteLine("DONE!");
        }
    }
}
