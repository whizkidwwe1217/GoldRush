using GoldRush.Core.Audit;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core.Repositories
{
    public class RepositoryManager<TKey> : IRepositoryManager<TKey>
    {
        public RepositoryManager(DbContext dbContext, Tenant tenant, IAuditManager<TKey> auditManager)
        {
            DbContext = dbContext;
            Tenant = tenant;
            AuditManager = auditManager;
        }

        public DbContext DbContext { get; set; }
        public Tenant Tenant { get; set; }
        public IAuditManager<TKey> AuditManager { get; set; }
    }
}