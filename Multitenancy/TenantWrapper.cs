namespace GoldRush.Multitenancy
{
    public class TenantWrapper<TTenant> : ITenant<TTenant> where TTenant : class
    {
        public TenantWrapper(TTenant tenant)
        {
            Value = tenant;
        }

        public TTenant Value { get; }
    }
}