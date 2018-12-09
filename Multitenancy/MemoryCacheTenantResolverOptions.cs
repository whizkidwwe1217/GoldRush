namespace GoldRush.Multitenancy
{
    public class MemoryCacheTenantResolverOptions
    {
        public MemoryCacheTenantResolverOptions()
        {
            EvictAllEntriesOnExpiry = true;
            DisposeOnEviction = true;
        }

        public bool EvictAllEntriesOnExpiry { get; set; }
        public bool DisposeOnEviction { get; set; }
    }
}