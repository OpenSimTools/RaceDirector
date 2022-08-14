﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDirector.Pipeline;
using RaceDirector.Plugin;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using RaceDirector.Config;

namespace RaceDirector;

[SupportedOSPlatform("windows")]
public static class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                IPAddressConverter.Register();
                configurationBuilder
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                PluginLoader.InitPlugins(context.Configuration, services);
            })
            .Build();

        var nodes = host.Services.GetServices<INode>();
        PipelineBuilder.LinkNodes(nodes);

        host.WaitForShutdown();
    }
}