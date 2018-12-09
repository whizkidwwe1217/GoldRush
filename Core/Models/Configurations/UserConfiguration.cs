using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations
{
    public class UserConfiguration : IntEntityTypeConfiguration<User>
    {
        public UserConfiguration() : base()
        {
        }

        public UserConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<User> builder)
        {
            builder.Property(e => e.UserName).HasMaxLength(50).IsRequired();
            builder.Property(e => e.ConfirmPassword).HasMaxLength(50);
            builder.Property(e => e.Password).HasMaxLength(50).IsRequired();
            builder.Property(e => e.PasswordHash).HasMaxLength(300);
            builder.Property(e => e.PhoneNumber).HasMaxLength(50);
            builder.Property(e => e.SecurityStamp).HasMaxLength(300);
            builder.Property(e => e.IsConfirmed).HasDefaultValue(false);
            builder.Property(e => e.IsSystemAdministrator).HasDefaultValue(false);
            builder.Property(e => e.RecoveryEmail).HasMaxLength(50);
            builder.Property(e => e.MobileNo).HasMaxLength(50);
            builder.Property(e => e.SecurityStamp).ValueGeneratedOnAdd();
            builder.HasIndex(u => new { u.TenantId, u.CompanyId, u.NormalizedUserName }).IsUnique();                
            builder.HasIndex(u => new { u.TenantId, u.CompanyId, u.NormalizedEmail }).IsUnique();                

            builder.Property(u => u.UserName).HasMaxLength(256);
            builder.Property(u => u.NormalizedUserName).HasMaxLength(256);
            builder.Property(u => u.Email).HasMaxLength(256);
            builder.Property(u => u.NormalizedEmail).HasMaxLength(256);
            
            // builder.HasMany<UserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            // builder.HasMany<UserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            // builder.HasMany<UserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            // builder.HasMany<UserToken>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();

            base.ConfigureBuilder(builder);
        }
    }
}