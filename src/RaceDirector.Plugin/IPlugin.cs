using Microsoft.Extensions.DependencyInjection;

namespace RaceDirector.Plugin
{
    public interface IPlugin
    {
        void Init(IServiceCollection services);
    }
}
