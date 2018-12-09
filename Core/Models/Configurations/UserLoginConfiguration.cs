using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations
{
    public class UserLoginConfiguration : IntEntityTypeConfiguration<UserLogin>
    {
        public UserLoginConfiguration()
        {
        }

        public UserLoginConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<UserLogin> builder)
        {
            builder.HasKey(l => new { l.LoginProvider, l.ProviderKey });

            builder
                .HasOne(ul => ul.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(ul => ul.UserId);

            base.ConfigureBuilder(builder);
        }
    }
}