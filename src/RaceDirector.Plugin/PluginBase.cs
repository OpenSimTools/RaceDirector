using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin;

public abstract class PluginBase<TConfig> : IPlugin
{
    public string Name => GetType().FullName;

    public void Init(IConfiguration configuration, IServiceCollection services)
    {
        var configSection = configuration.GetSection(Name);
        var pluginConfig = configSection.Get<TConfig>();
        Init(pluginConfig, services);
    }

    protected abstract void Init(TConfig pluginConfig, IServiceCollection services);
}