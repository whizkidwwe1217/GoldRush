namespace GoldRush.Multitenancy
{
    public class TenantPipelineBuilderContext<TTenant> where TTenant : class
    {
        public TenantContext<TTenant> TenantContext { get; set; }
        public TTenant Tenant { get; set; }
    }
}