using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace AspNetCoreEnricher.Internal
{
    internal sealed class EnricherStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // Register enricher middleware at the start of the pipeline.
                // Can't go at the end because terminal middleware, such as endpoint execution,
                // means that middleware afterwards won't run.
                builder.UseMiddleware<RunEnrichersMiddleware>();
                next(builder);
            };
        }
    }
}