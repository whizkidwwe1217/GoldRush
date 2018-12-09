using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure.Multitenancy
{
    public class TenantActiveDbContext : IActiveDbContext 
    {
        private DbContext dbContext;

        public TenantActiveDbContext(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DbContext DbContext { get => dbContext; }
    }
}