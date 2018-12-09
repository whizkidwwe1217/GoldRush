using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations
{
    public class UserClaimConfiguration : IntEntityTypeConfiguration<UserClaim>
    {
        public UserClaimConfiguration()
        {
        }

        public UserClaimConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<UserClaim> builder)
        {
            builder
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserClaims)
                .HasForeignKey(uc => uc.UserId);

            base.ConfigureBuilder(builder);
        }
    }
}