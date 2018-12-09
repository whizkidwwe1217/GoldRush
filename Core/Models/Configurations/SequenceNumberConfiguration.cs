using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldRush.Core.Models.Configurations.Common;

namespace GoldRush.Core.Models.Configurations
{
    public class SequenceNumberConfiguration : IntEntityTypeConfiguration<SequenceNumber>
    {
        public SequenceNumberConfiguration() : base()
        {
        }

        public SequenceNumberConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<SequenceNumber> builder)
        {
            builder.Property(e => e.LeftPadding).HasDefaultValue(0);
            builder.Property(e => e.RightPadding).HasDefaultValue(0);
            builder.Property(e => e.LeftPaddingChar).HasMaxLength(1).HasDefaultValue('0');
            builder.Property(e => e.RightPaddingChar).HasMaxLength(1).HasDefaultValue('0');
            builder.Property(e => e.Prefix).HasMaxLength(50);
            builder.Property(e => e.Suffix).HasMaxLength(50);
            builder.Property(e => e.CycleSequence).HasDefaultValue(false);
            builder.Property(e => e.EndCyclePosition).HasDefaultValue(0);
            builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Remarks).HasMaxLength(300);
            builder.Property(e => e.StartingValue).HasDefaultValue(0);
            builder.Property(e => e.ResetValue).HasDefaultValue(0);
            builder.HasIndex(e => new { e.TenantId, e.CompanyId, e.Name }).IsUnique();
            base.ConfigureBuilder(builder);
        }
    }
}