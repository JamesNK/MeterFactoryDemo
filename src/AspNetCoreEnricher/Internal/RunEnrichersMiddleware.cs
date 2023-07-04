using Microsoft.AspNetCore.Diagnostics;
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

                // Tags feature will be null if no one is listening to the counter.
                // No point annotating measurement with tags if they'll never be used.
                var tags = httpContext.Features.Get<IHttpMetricsTagsFeature>()?.Tags;
                if (tags != null)
                {
                    Exception? exception = null;
                    if (httpContext.Items.TryGetValue("__Exception", out var e))
                    {
                        // If the exception was unhandled.
                        exception = (Exception?)e;
                    }
                    else
                    {
                        // This feature is set by ExceptionHandler middleware.
                        exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
                    }

                    foreach (var enricher in _enrichers)
                    {
                        enricher.Enrich(tags, c, exception);
                    }
                }
                return Task.CompletedTask;
            }, httpContext);

            try
            {
                // Continue invoking middleware pipeline.
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Apps usually have ExceptionHandler middleware setup so unlikely to hit this point.
                // Developer exception middlware catches the exception and prevents it being set here.
                httpContext.Items["__Exception"] = ex;

                // Should throw exception here but test host doesn't appear to run OnCompleted
                // if there is an unhandled exception.
                // throw;

                httpContext.Response.StatusCode = 500;
            }
        }
    }
}