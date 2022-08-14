using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin;

/// <summary>
/// Provides typed configuration and disabling of plugins. Most plugins should implement this class.
/// </summary>
/// <typeparam name="TConfig">Plugin configuration type</typeparam>
public abstract class PluginBase<TConfig> : IPlugin where TConfig : PluginBase.Config
{
    public string Name => GetType().FullName;

    public void Init(IConfiguration configuration, IServiceCollection services)
    {
        var configSection = configuration.GetSection(Name);
        var pluginConfig = configSection.Get<TConfig>();
        if (pluginConfig.Enabled)
            Init(pluginConfig, services);
    }

    protected abstract void Init(TConfig pluginConfig, IServiceCollection services);
}

public static class PluginBase
{
    public class Config
    {
        public bool Enabled { get; set; } = true;
    }
}