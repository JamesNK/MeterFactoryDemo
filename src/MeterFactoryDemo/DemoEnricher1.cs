using AspNetCoreEnricher;

namespace MeterFactoryDemo
{
    public sealed class DemoEnricher1 : IEnricher
    {
        public void Enrich(ICollection<KeyValuePair<string, object?>> tags, HttpContext httpContext, Exception? exception)
        {
            tags.Add(new KeyValuePair<string, object?>("TagName1", "Value!"));
            if (exception != null)
            {
                tags.Add(new KeyValuePair<string, object?>("ExceptionMessage", exception.Message));
            }
        }
    }
}
