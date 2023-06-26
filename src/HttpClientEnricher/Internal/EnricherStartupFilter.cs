using Microsoft.Extensions.Http;

namespace HttpClientEnricher.Internal
{
    internal sealed class EnricherStartupFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly List<IEnricher> _enrichers;

        public EnricherStartupFilter(IEnumerable<IEnricher> enrichers)
        {
            _enrichers = enrichers.ToList();
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                next(builder);

                builder.AdditionalHandlers.Add(new EnricherMessageHandler(_enrichers));
            };
        }
    }
}