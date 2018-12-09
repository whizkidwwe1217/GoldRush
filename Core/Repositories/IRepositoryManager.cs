using GoldRush.Core.Audit;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core.Repositories
{
    public interface IRepositoryManager<TKey>
    {
        DbContext DbContext { get; set; }
        Tenant Tenant { get; set; }
        IAuditManager<TKey> AuditManager { get; set; }
    }
}