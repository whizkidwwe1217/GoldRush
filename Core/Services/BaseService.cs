using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Repositories;

namespace GoldRush.Core.Services
{
    public abstract class BaseService<TKey, TEntity, TRoleRepository> : BaseService<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
        where TRoleRepository : IRepository<TKey, TEntity>
    {
        public new TRoleRepository Repository { get; set; }
        public BaseService(TRoleRepository repository) : base(repository)
        {
            Repository = repository;
        }
    }

    public abstract class BaseService<TKey, TEntity> : IService<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
    {
        public BaseService(IRepository<TKey, TEntity> repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public IRepository<TKey, TEntity> Repository { get; private set; }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task AddAsync(TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await Repository.AddAsync(entity, cancellationToken);
        }

        public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.GetAsync(id, cancellationToken);
        }

        public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.GetAsync(predicate, cancellationToken);
        }

        public virtual async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.ListAsync(cancellationToken);
        }

        public virtual void Remove(TEntity entity)
        {
            Repository.Remove(entity);
        }

        public virtual async Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Repository.SaveAsync(cancellationToken);
        }

        public async Task<SearchResponseData<TEntity>> SearchAsync(int? currentPage = 1, int? pageSize = 100,
            string filter = "", string sort = "", string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.SearchAsync(currentPage, pageSize, filter, sort, fields, cancellationToken);
        }

        public async Task<SearchResponseData<TEntity>> SearchAsync(SearchParams parameter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.SearchAsync(parameter, cancellationToken);
        }

        public virtual async Task<TEntity> SeekAsync(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.SeekAsync(id, cancellationToken);
        }

        public virtual async Task UpdateAsync(TKey id, TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await Repository.UpdateAsync(id, entity, cancellationToken);
        }
    }
}