using Microsoft.AspNetCore.Http;

namespace AspNetCoreEnricher
{
    public interface IEnricher
    {
        void Enrich(ICollection<KeyValuePair<string, object?>> tags, HttpContext httpContext, Exception? exception);
    }
}