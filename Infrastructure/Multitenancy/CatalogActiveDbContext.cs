using GoldRush.Core;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure.Multitenancy
{
    public class CatalogActiveDbContext : IActiveDbContext 
    {
        private DbContext dbContext;

        public CatalogActiveDbContext(ICatalogDataSource dataSource)
        {
            dbContext = dataSource.DbContext;
        }

        public DbContext DbContext { get => dbContext; }
    }
}