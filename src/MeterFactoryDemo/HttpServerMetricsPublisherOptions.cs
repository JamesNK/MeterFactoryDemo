namespace MeterFactoryDemo
{
    public class HttpServerMetricsPublisherOptions
    {
        public Dictionary<string, string> Mapping { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
    }
}
