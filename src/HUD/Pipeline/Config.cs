using System.Net;

namespace RaceDirector.Plugin.HUD.Pipeline.Config
{
    public interface IDashboardServerConfig
    {
        IPAddress address { get; }
        int port { get; }
    }

    // TODO remove when config done
    public record DashboardServerConfig(IPAddress address, int port = 8070) : IDashboardServerConfig;
}
