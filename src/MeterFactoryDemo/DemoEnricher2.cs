using AspNetCoreEnricher;

namespace MeterFactoryDemo
{
    public sealed class DemoEnricher2 : IEnricher
    {
        public void Enrich(ICollection<KeyValuePair<string, object?>> tags)
        {
            tags.Add(new KeyValuePair<string, object?>("TagName2", "Value!"));
        }
    }
}
