using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations
{
    public class UserRoleConfiguration : IntEntityTypeConfiguration<UserRole>
    {
        public UserRoleConfiguration()
        {
        }

        public UserRoleConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasOne(u => u.User)
                .WithMany(u => u.UserRoles)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasKey(ur => new { ur.UserId, ur.RoleId });


            builder
                .HasOne(ur => ur.Role)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            builder
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            base.ConfigureBuilder(builder);
        }
    }
}