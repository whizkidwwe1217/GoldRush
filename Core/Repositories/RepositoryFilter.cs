using GoldRush.Core.Models.Common;
using GoldRush.Core.Pipeline;

namespace GoldRush.Core.Repositories
{
    public abstract class RepositoryFilter<TKey, TEntity, TRepository> : RepositoryFilter<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
        where TRepository : IRepository<TKey, TEntity>
    {
        public new TRepository Repository { get; set; }
        public RepositoryFilter(TRepository repository) : base(repository)
        {
            Repository = repository;
        }
    }

    public abstract class RepositoryFilter<TKey, TEntity> : AsyncFilter<TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
    {
        private readonly IRepository<TKey, TEntity> repository;

        public RepositoryFilter(IRepository<TKey, TEntity> repository)
        {
            this.repository = repository;
        }

        public IRepository<TKey, TEntity> Repository => repository;
    }
}