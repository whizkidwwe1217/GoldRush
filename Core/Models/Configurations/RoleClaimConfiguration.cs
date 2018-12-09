using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations
{
    public class RoleClaimConfiguration : IntEntityTypeConfiguration<RoleClaim>
    {
        public RoleClaimConfiguration()
        {
        }

        public RoleClaimConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<RoleClaim> builder)
        {
            builder
                .HasOne(rc => rc.Role)
                .WithMany(u => u.RoleClaims)
                .HasForeignKey(rc => rc.RoleId);

            base.ConfigureBuilder(builder);
        }
    }
}