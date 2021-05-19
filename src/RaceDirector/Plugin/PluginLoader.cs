using System.Collections.Generic;

namespace RaceDirector.Plugin
{
    public static class PluginLoader
    {
        public static IEnumerable<IPlugin> InstantiatePlugins()
        {
            return new IPlugin[] { new DefaultPlugin(), new HUD.Plugin() };
        }
    }
}
