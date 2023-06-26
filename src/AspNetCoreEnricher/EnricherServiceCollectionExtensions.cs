using AspNetCoreEnricher.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace AspNetCoreEnricher
{
    public static class EnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddEnricher<TEnricher>(this IServiceCollection services) where TEnricher : class, IEnricher
        {
            // Register startup filter that adds middleware when an enricher is added.
            // It's ok if this is called multiple times, TryAdd ensures it is only added once.
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, EnricherStartupFilter>());
            // Register enricher type.
            services.TryAddEnumerable(ServiceDescriptor.Transient<IEnricher, TEnricher>());
            return services;
        }
    }
}