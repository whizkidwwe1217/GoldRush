using System;
using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models.Configurations.Common
{
    public abstract class GuidEntityTypeConfiguration<TEntity> : EntityTypeConfiguration<TEntity, Guid>
        where TEntity : class, IBaseEntity<Guid>
    {
        public GuidEntityTypeConfiguration()
        {
        }

        public GuidEntityTypeConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }
    }
}