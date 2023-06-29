namespace HttpClientEnricher.Internal
{
    internal sealed class EnricherMessageHandler : DelegatingHandler
    {
        private readonly List<IEnricher> _enrichers;

        public EnricherMessageHandler(IEnumerable<IEnricher> enrichers)
        {
            _enrichers = enrichers.ToList();
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