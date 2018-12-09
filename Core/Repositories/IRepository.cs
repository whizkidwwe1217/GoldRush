using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core.Repositories
{
    public interface IRepository<TKey, TEntity> where TEntity : class, IBaseEntity<TKey>, new()
    {
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
        IRepositoryManager<TKey> RepositoryManager { get; set; }
        DbContext DbContext { get; }
        bool UseTransaction { get; set; }
        Pipeline<TEntity> CreationPipeline { get; set; }
        Pipeline<TEntity> ModificationPipeline { get; set; }
    }
}