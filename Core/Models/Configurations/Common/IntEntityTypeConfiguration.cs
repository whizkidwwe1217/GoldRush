using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models.Configurations.Common
{
    public abstract class IntEntityTypeConfiguration<TEntity> : EntityTypeConfiguration<TEntity, int>
        where TEntity: class, IBaseEntity<int>
    {
        public IntEntityTypeConfiguration()
        {
        }

        public IntEntityTypeConfiguration(bool useRowVersion) : base(useRowVersion)
        {
        }
    }
}