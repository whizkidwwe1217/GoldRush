using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Repositories;

namespace GoldRush.Core.Services
{
    public interface IService<TKey, TEntity> where TEntity : class, IBaseEntity<TKey>, new()
    {
        IRepository<TKey, TEntity> Repository { get; }
        Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> SeekAsync(TKey id, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken));
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        void Remove(TEntity entity);
        Task UpdateAsync(TKey id, TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<SearchResponseData<TEntity>> SearchAsync(int? currentPage = 1, int? pageSize = 100,
            string filter = "", string sort = "", string fields = "",
            CancellationToken cancellationToken = default(CancellationToken));
        Task<SearchResponseData<TEntity>> SearchAsync(SearchParams parameter,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}