using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PitCrew.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton(_ => context.Configuration.Get<Config>())
                    .AddSingleton<PitCrewServer>();
            })
            .Build();

        var server = host.Services.GetRequiredService<PitCrewServer>();
        server.Start();

        host.WaitForShutdown();
    }
}