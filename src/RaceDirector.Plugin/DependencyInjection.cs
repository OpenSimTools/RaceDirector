using Microsoft.Extensions.DependencyInjection;
using System;

namespace RaceDirector.DependencyInjection
{
    public static class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddSingletonWithInterfaces<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.AddSingleton(implementationFactory).AddInterfaces<TService>();
        }

        public static IServiceCollection AddSingletonWithInterfaces<TService>(this IServiceCollection services)
    where TService : class
        {
            return services.AddSingleton<TService>().AddInterfaces<TService>();
        }

        public static IServiceCollection AddTransientWithInterfaces<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.AddTransient(implementationFactory).AddInterfaces<TService>();
        }

        public static IServiceCollection AddTransientWithInterfaces<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddTransient<TService>().AddInterfaces<TService>();
        }

        public static IServiceCollection AddInterfaces<TService>(this IServiceCollection services)
            where TService : class
        {
            foreach (var i in typeof(TService).GetInterfaces())
            {
                services.AddTransient(i, s => s.GetRequiredService<TService>());
            }
            return services;
        }
    }
}
