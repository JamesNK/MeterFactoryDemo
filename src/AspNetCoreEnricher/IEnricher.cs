namespace AspNetCoreEnricher
{
    public interface IEnricher
    {
        void Enrich(ICollection<KeyValuePair<string, object?>> tags);
    }
}