using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldRush.Core.Models.Configurations.Common;

namespace GoldRush.Core.Models.Configurations
{
    public class RoleConfiguration : IntEntityTypeConfiguration<Role>
    {
        public RoleConfiguration() : base()
        {
        }

        public RoleConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<Role> builder)
        {
            builder.Property(e => e.Description).HasMaxLength(200);
            builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
            builder.Property(e => e.IsSystemAdministrator).HasDefaultValue(false);

            builder.Property(u => u.Name).HasMaxLength(256);
            builder.Property(u => u.NormalizedName).HasMaxLength(256);
            builder.HasIndex(r => new { r.TenantId, r.CompanyId, r.NormalizedName }).IsUnique();
            
            // builder.HasMany<RoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
            base.ConfigureBuilder(builder);
        }
    }
}