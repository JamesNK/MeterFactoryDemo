namespace HttpClientEnricher.Internal
{
    internal sealed class EnricherMessageHandler : DelegatingHandler
    {
        private readonly List<IEnricher> _enrichers;

        public EnricherMessageHandler(List<IEnricher> enrichers)
        {
            _enrichers = enrichers;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            /*
             * Callback API is not available yet.
            request.Options.AddMetricsCallback(context =>
            {
                foreach (var enricher in _enrichers)
                {
                    enricher.Enrich(context.Tags);
                }
            });
            */

            return base.SendAsync(request, cancellationToken);
        }
    }
}