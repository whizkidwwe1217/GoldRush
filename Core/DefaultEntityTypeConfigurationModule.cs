using GoldRush.Core.Audit;
using GoldRush.Core.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core
{
    public class DefaultEntityTypeConfigurationModule : IEntityTypeConfigurationModule
    {
        public string Name => "Default Entity Configuration Modules";

        public void ApplyConfigurations(ModelBuilder builder, bool useRowVersion)
        {
            builder.ApplyConfiguration(new TenantConfiguration(useRowVersion));
            builder.ApplyConfiguration(new CompanyConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserConfiguration(useRowVersion));
            builder.ApplyConfiguration(new RoleConfiguration(useRowVersion));
            builder.ApplyConfiguration(new RoleClaimConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserRoleConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserLoginConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserTokenConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserClaimConfiguration(useRowVersion));
            builder.ApplyConfiguration(new SubscriptionConfiguration(useRowVersion));
            builder.ApplyConfiguration(new AuditEntityConfiguration(useRowVersion));
            builder.ApplyConfiguration(new SequenceNumberConfiguration(useRowVersion));
        }
    }
}