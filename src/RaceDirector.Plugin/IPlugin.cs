using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin
{
    public interface IPlugin
    {
        void Init(IConfiguration configuration, IServiceCollection services);
    }
}
