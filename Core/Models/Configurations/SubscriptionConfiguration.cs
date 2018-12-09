using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldRush.Core.Models.Configurations.Common;

namespace GoldRush.Core.Models.Configurations
{
    public class SubscriptionConfiguration : GuidEntityTypeConfiguration<Subscription>
    {
        public SubscriptionConfiguration()
        {
        }

        public SubscriptionConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<Subscription> builder)
        {
            builder.Property(e => e.TenantId).IsRequired();
            builder.Property(e => e.SubscriptionType).HasMaxLength(50).HasConversion<string>().IsRequired();
            base.ConfigureBuilder(builder);
        }
    }
}