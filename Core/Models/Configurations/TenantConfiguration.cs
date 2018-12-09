using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core.Models.Configurations
{
    public class TenantConfiguration : GuidEntityTypeConfiguration<Tenant>
    {
        public TenantConfiguration()
        {
        }

        public TenantConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasIndex(e => e.Name).IsUnique();
            builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(e => e.HostName).IsUnique();
            builder.Property(e => e.Active).HasDefaultValue(true);
            builder.Property(e => e.HostName).HasMaxLength(300).IsRequired();
            builder.Property(e => e.Engine).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(e => e.Edition).HasMaxLength(20);
            builder.Property(e => e.ConnectionString).HasMaxLength(400).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(300);
            builder.Property(e => e.Theme).HasMaxLength(50).HasDefaultValue("Default");
            base.ConfigureBuilder(builder);
        }
    }
}