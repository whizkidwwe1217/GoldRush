using System;
using System.Threading.Tasks;
using StructureMap;

namespace GoldRush.Multitenancy
{
    public class TenantContainerBuilder<TTenant> : ITenantContainerBuilder<TTenant>
    {
        public TenantContainerBuilder(IContainer container, Action<TTenant, ConfigurationExpression> configure)
        {
            Container = container;
            Configure = configure;
        }

        public IContainer Container { get; }
        public Action<TTenant, ConfigurationExpression> Configure { get; }

        public virtual async Task<IContainer> BuildAsync(TTenant tenant)
        {
            var tenantContainer = Container.CreateChildContainer();
            tenantContainer.Configure(config => Configure(tenant, config));
            return await Task.FromResult(tenantContainer);
        }
    }
}