using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldRush.Core.Models.Configurations.Common;

namespace GoldRush.Core.Models.Configurations
{
    public class CompanyConfiguration : GuidEntityTypeConfiguration<Company>
    {
        public CompanyConfiguration()
        {
        }

        public CompanyConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<Company> builder)
        {
            builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(e => e.Code).IsUnique();
            builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
            base.ConfigureBuilder(builder);
        }
    }
}