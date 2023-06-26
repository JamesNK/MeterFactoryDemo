using HttpClientEnricher.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace HttpClientEnricher
{
    public static class HttpClientEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddEnricher<TEnricher>(this IServiceCollection services) where TEnricher : class, IEnricher
        {
            // Register startup filter that adds middleware when an enricher is added.
            // It's ok if this is called multiple times, TryAdd ensures it is only added once.
            services.TryAddEnumerable(ServiceDescriptor.Transient<IHttpMessageHandlerBuilderFilter, EnricherStartupFilter>());
            // Register enricher type.
            services.TryAddEnumerable(ServiceDescriptor.Transient<IEnricher, TEnricher>());
            return services;
        }
    }
}