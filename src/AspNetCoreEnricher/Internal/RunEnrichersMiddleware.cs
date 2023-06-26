using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace AspNetCoreEnricher.Internal
{
    public sealed class RunEnrichersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<IEnricher> _enrichers;

        public RunEnrichersMiddleware(RequestDelegate next, IEnumerable<IEnricher> enrichers)
        {
            _next = next;
            _enrichers = enrichers.ToList();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Do enrichment when response is completed.
            // This is as close as possible to the counter being written.
            httpContext.Response.OnCompleted(s =>
            {
                var c = (HttpContext)s!;
                var tags = httpContext.Features.Get<IHttpMetricsTagsFeature>()?.Tags;
                if (tags != null)
                {
                    foreach (var enricher in _enrichers)
                    {
                        enricher.Enrich(tags);
                    }
                }
                return Task.CompletedTask;
            }, httpContext);

            await _next(httpContext);
        }
    }
}