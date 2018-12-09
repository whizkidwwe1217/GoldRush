using GoldRush.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations.Common
{
    public abstract class EntityTypeConfiguration<TEntity, TKey> : RowVersionEntityTypeConfiguration<TEntity>
        where TEntity : class, IBaseEntity<TKey>
    {
        public EntityTypeConfiguration()
        {
        }

        public EntityTypeConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }

        public override void ConfigureBuilder(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            if (typeof(ICanActivate).IsAssignableFrom(typeof(TEntity)))
            {
                builder.Property(e => ((ICanActivate)e).Active).HasDefaultValue(true);
            }

            if (typeof(ICanSoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                builder.Property(e => ((ICanSoftDelete)e).Deleted).HasDefaultValue(false);
            }
        }
    }
}