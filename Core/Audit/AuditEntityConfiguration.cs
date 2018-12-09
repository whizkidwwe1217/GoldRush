using GoldRush.Core.Models.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Audit
{
    public class AuditEntityConfiguration : IntEntityTypeConfiguration<AuditEntity>
    {
        public AuditEntityConfiguration()
        {
        }

        public AuditEntityConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<AuditEntity> builder)
        {
            builder.Property(e => e.Action).HasMaxLength(50);
            builder.Property(e => e.Changes).HasMaxLength(8000);
            builder.Property(e => e.CurrentValue).HasMaxLength(8000);
            builder.Property(e => e.PreviousValue).HasMaxLength(8000);
            builder.Property(e => e.Source).HasMaxLength(100);
            builder.Property(e => e.Key).HasMaxLength(50);
            builder.Property(e => e.Namespace).HasMaxLength(100);
            builder.Property(e => e.Model).HasMaxLength(100);
            base.ConfigureBuilder(builder);
        }
    }
}