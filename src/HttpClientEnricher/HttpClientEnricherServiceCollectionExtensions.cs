using HttpClientEnricher.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HttpClientEnricher
{
    public static class HttpClientEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddEnricher<TEnricher>(this IServiceCollection services) where TEnricher : class, IEnricher
        {
            // Register enricher type.
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IEnricher, TEnricher>());
            return services;
        }
    }

    public static class HttpClientBuilderEnricherExtensions
    {
        public static IHttpClientBuilder AddHttpEnrichment(this IHttpClientBuilder builder)
        {
            builder.AddHttpMessageHandler<EnricherMessageHandler>();
            return builder;
        }
    }
}