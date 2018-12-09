using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Audit;
using GoldRush.Core.Models;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core.Repositories
{
    public abstract class BaseRepository<TKey, TEntity> : IRepository<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, ICanAudit, new()
    {
        private const string Action = "Update";
        private TEntity modifiedEntity;
        private readonly Stack<AuditEntity> auditEntitiesTracker = new Stack<AuditEntity>();
        private IRepositoryManager<TKey> repositoryManager;
        public IRepositoryManager<TKey> RepositoryManager { get => repositoryManager; set => repositoryManager = value; }
        public DbContext DbContext => RepositoryManager.DbContext;
        public Tenant Tenant => RepositoryManager.Tenant;
        protected virtual string GetCompanyIdString() { return string.Empty; }
        public virtual bool UseTransaction { get; set; } = true;
        public Pipeline<TEntity> CreationPipeline { get; set; }
        public Pipeline<TEntity> ModificationPipeline { get; set; }

        public BaseRepository(IRepositoryManager<TKey> repositoryManager)
        {
            this.repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
            CreationPipeline = new Pipeline<TEntity>();
            ModificationPipeline = new Pipeline<TEntity>();
            var type = this.GetType();
            var mi = type.GetMethod("ConfigurePipelines");
            if (mi != null)
            {
                mi.Invoke(this, new object[] { CreationPipeline, EntityState.Added });
                mi.Invoke(this, new object[] { ModificationPipeline, EntityState.Modified });
            }
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>().Where(predicate)).AsNoTracking().AnyAsync(cancellationToken);
        }

        protected virtual void AddEntityTimestampValues(TEntity entity, AuditActions action)
        {
            if (action == AuditActions.Update)
                entity.DateModified = DateTime.UtcNow;
            else if (action == AuditActions.New)
                entity.DateCreated = DateTime.UtcNow;
            else if (action == AuditActions.Delete)
                entity.DateDeleted = DateTime.UtcNow;
        }

        protected virtual void AddEntityDefaultTenantValues(TEntity entity)
        {
            if (typeof(ITenantEntity<Guid, TKey>).IsAssignableFrom(typeof(TEntity)))
            {
                ((ITenantEntity<Guid, TKey>)entity).TenantId = Tenant.Id;
            }
        }

        protected virtual async Task AuditChanges(AuditActions action, object id, TEntity previousEntity, TEntity currentEntity)
        {
            if (typeof(ICanAudit).IsAssignableFrom(typeof(TEntity)))
            {
                var auditEntity = RepositoryManager.AuditManager.Audit(action, Tenant.Id.ToString(), GetCompanyIdString(), currentEntity.GetType(), id.ToString(), previousEntity, currentEntity);
                if (auditEntity != null)
                {
                    if (action == AuditActions.New)
                        auditEntitiesTracker.Push(auditEntity);
                    else
                        await DbContext.Set<AuditEntity>().AddAsync(auditEntity);
                }
            }
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AddEntityDefaultTenantValues(entity);
            entity = await CreationPipeline.Execute(entity);
            AddEntityTimestampValues(entity, AuditActions.New);
            await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            modifiedEntity = entity;
            await AuditChanges(AuditActions.New, entity.Id, default(TEntity), entity);
        }

        private bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        private bool IsItNew(TEntity entity) => !DbContext.Entry<TEntity>(entity).IsKeySet;

        public virtual async Task UpdateAsync(TKey id, TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var originalEntity = await Set<TEntity>().AsNoTracking().Where(x => Compare<TKey>(x.Id, id)).FirstOrDefaultAsync();
            var e = DbContext.Entry<TEntity>(entity);
            AddEntityDefaultTenantValues(entity);
            entity = await ModificationPipeline.Execute(entity);
            AddEntityTimestampValues(entity, AuditActions.Update);
            DbContext.Update<TEntity>(entity);
            await AuditChanges(AuditActions.Update, id, originalEntity, entity);
        }

        protected virtual async Task SaveAudit(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (auditEntitiesTracker.Count > 0)
            {
                var auditEntity = auditEntitiesTracker.Pop();
                auditEntity.Key = modifiedEntity?.Id?.ToString();
                await DbContext.Set<AuditEntity>().AddAsync(auditEntity);
                await DbContext.SaveChangesAsync();
            }
            ClearAuditEntityTracker();
        }

        private void ClearAuditEntityTracker()
        {
            auditEntitiesTracker.Clear();
            modifiedEntity = null;
        }

        public virtual async Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (UseTransaction)
                await SaveInTransaction(cancellationToken);
            else
                await Save(cancellationToken);
        }

        protected virtual async Task Save(CancellationToken cancellationToken = default(CancellationToken))
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            await SaveAudit(cancellationToken);
        }

        protected virtual async Task SaveInTransaction(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var transaction = await DbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await Save(cancellationToken);
                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ClearAuditEntityTracker();
                    throw this.CreateInnerException("This record has been modified by another user", ex);
                }
                catch (Exception ex)
                {
                    ClearAuditEntityTracker();
                    throw this.CreateInnerException("Error saving record", ex);
                }
            }
        }


        public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>()).FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }

        public virtual async Task<TEntity> SeekAsync(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>()).AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        }

        public virtual async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>()).AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual void Remove(TEntity entity)
        {
            DbContext.Set<TEntity>().Remove(entity);
        }

        public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>()).FirstAsync(predicate, cancellationToken);
        }

        public async virtual Task<SearchResponseData<TEntity>> SearchAsync(int? currentPage = 1, int? pageSize = 100,
            string filter = "", string sort = "", string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>())
                .Search<TEntity, TKey>(filter)
                .ToListAsync<TEntity>(currentPage, pageSize, fields, cancellationToken);
        }

        public async virtual Task<SearchResponseData<TEntity>> SearchAsync(
            SearchParams parameter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await (GetTenantEntity() ?? DbContext.Set<TEntity>())
                .Search<TEntity, TKey>(parameter)
                .ToListAsync<TEntity>(parameter.CurrentPage, parameter.PageSize,
                    parameter.Fields, cancellationToken);
        }

        public virtual IQueryable<TEntity> Set()
        {
            return (GetTenantEntity() ?? DbContext.Set<TEntity>());
        }

        public virtual IQueryable<TEntity> Set<TDBEntity>() where TDBEntity : class, TEntity, new()
        {
            return (GetTenantEntity() ?? DbContext.Set<TDBEntity>());
        }

        protected virtual IQueryable<TEntity> GetTenantEntity()
        {
            if (Tenant != null)
            {
                if (typeof(ITenantEntity<Guid, TKey>).IsAssignableFrom(typeof(TEntity)))
                {
                    return DbContext.Set<TEntity>()
                        .Cast<ITenantEntity<Guid, TKey>>()
                        .Where(e => e.TenantId == Tenant.Id && !e.Deleted && e.Active)
                        .Cast<TEntity>();
                }
            }

            return null;
        }
    }
}