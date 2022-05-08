using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDirector.Pipeline;
using RaceDirector.Plugin;
using System;
using System.Runtime.Versioning;

namespace RaceDirector
{
    [SupportedOSPlatform("windows")]
    static class Program
    {
        static void Main(string[] args)
        {
            var plugins = PluginLoader.InstantiatePlugins();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    foreach (var p in plugins)
                        p.Init(services);
                })
                .Build();

            var nodes = host.Services.GetServices<INode>();
            PipelineBuilder.LinkNodes(nodes);

            host.WaitForShutdown();
        }
    }
}
