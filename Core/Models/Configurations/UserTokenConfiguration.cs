using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations
{
    public class UserTokenConfiguration : IntEntityTypeConfiguration<UserToken>
    {
        public UserTokenConfiguration()
        {
        }

        public UserTokenConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<UserToken> builder)
        {
            builder.HasKey(l => new { l.UserId, l.LoginProvider, l.Name });

            builder
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTokens)
                .HasForeignKey(ut => ut.UserId);

            base.ConfigureBuilder(builder);
        }
    }
}