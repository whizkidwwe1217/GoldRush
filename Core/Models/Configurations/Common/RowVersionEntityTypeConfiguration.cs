using GoldRush.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldRush.Core.Models.Configurations.Common
{
    public abstract class RowVersionEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class
    {
        private bool useRowVersion = true;

        public RowVersionEntityTypeConfiguration()
            : this(true)
        {
        }

        public RowVersionEntityTypeConfiguration(bool useRowVersion)
        {
            this.useRowVersion = useRowVersion;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            if (useRowVersion)
            {
                builder.Property(e => ((IConcurrencyEntity)e).ConcurrencyStamp).IsRowVersion();
            }
            else
            {
                builder.Property(e => ((IConcurrencyEntity)e).ConcurrencyTimeStamp)
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            }
            UseNamingConvention(builder);
            ConfigureBuilder(builder);
        }

        // By default, entity names will be pluralized. Override this method if to use another naming convention.
        public virtual void UseNamingConvention(EntityTypeBuilder<TEntity> builder) => builder.ToTable(Inflector.Pluralize(typeof(TEntity).Name));

        public abstract void ConfigureBuilder(EntityTypeBuilder<TEntity> builder);
    }
}