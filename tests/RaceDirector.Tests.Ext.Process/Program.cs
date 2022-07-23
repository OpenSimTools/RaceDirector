using System;
using System.Diagnostics;
using System.Threading;

namespace RaceDirector.Tests.Ext.Process;

class Program
{
    static void Main(string[] args)
    {
        var timeToWait = args[1];
        Debug.Write("Waiting " + timeToWait + " seconds... ");
        Thread.Sleep(TimeSpan.FromSeconds(Convert.ToInt32(timeToWait)));
        Debug.WriteLine("DONE!");
    }
}