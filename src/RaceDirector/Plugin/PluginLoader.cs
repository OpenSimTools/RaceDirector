using System.Collections.Generic;
using System.Runtime.Versioning;

namespace RaceDirector.Plugin
{
    public static class PluginLoader
    {
        [SupportedOSPlatform("windows")]

        public static IEnumerable<IPlugin> InstantiatePlugins()
        {
            return new IPlugin[] { new DefaultPlugin(), new HUD.Plugin() };
        }
    }
}
