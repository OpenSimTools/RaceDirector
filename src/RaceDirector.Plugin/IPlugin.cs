using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin;

public interface IPlugin
{
    public string Name { get; }

    void Init(IConfiguration configuration, IServiceCollection services);
}