using RaceDirector.Plugin.HUD.Pipeline;
using RaceDirector.Pipeline.Games.R3E.Contrib;
using RaceDirector.Pipeline.Games.R3E.Contrib.Data;
using RaceDirector.Pipeline.Utils;
using System;
using System.Runtime.Versioning;
using Newtonsoft.Json.Linq;
using JsonDiffPatchDotNet;
using System.Text;
using System.IO;
using RaceDirector.Pipeline.Games.R3E;

namespace E2ETest
{
    class Program
    {
        [SupportedOSPlatform("windows")]
        static void Main(string[] args)
        {
            int? loopWaitMs = null;
            
            if (args.Length == 2 && args[0] == "--loopMs")
                loopWaitMs = int.Parse(args[1]);

            using (var mmReader = new MemoryMappedFileReader<Shared>(Constant.SharedMemoryName))
            {
                var telemetry = new Telemetry();
                while (true)
                {
                    if (loopWaitMs is null)
                    {
                        Console.Write("> ");
                        var command = Console.ReadLine();
                        if (command == "q" || command == "quit" || command == "exit")
                            break;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(loopWaitMs.Value);
                    }

                    var rrTelemetry = mmReader.Read();
                    var rdTelemetry = telemetry.Transform(rrTelemetry);

                    var mmJsonBytes = SharedDataJsonWriter.ToJson(rrTelemetry);
                    var wsJsonBytes = R3EDashTransformer.ToR3EDash(rdTelemetry);

                    var jdp = new JsonDiffPatch();
                    var mmJson = JToken.Parse(Encoding.UTF8.GetString(mmJsonBytes));
                    var wsJson = JToken.Parse(Encoding.UTF8.GetString(wsJsonBytes));
                    var diff = jdp.Diff(mmJson, wsJson);

                    if (diff is null)
                    {
                        Console.WriteLine("EVERYTHING MATCHES! YAY!");
                    }
                    else
                    {
                        var id = DateTime.Now.Second.ToString();
                        Console.WriteLine(id);
                        Console.WriteLine(diff.ToString());
                        File.WriteAllTextAsync($"{id}-left.json", mmJson.ToString());
                        File.WriteAllTextAsync($"{id}-right.json", wsJson.ToString());
                    }
                }
            }
        }
    }
}
