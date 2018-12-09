using Microsoft.EntityFrameworkCore;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public class PostgreSqlCatalogDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<PostgreSqlCatalogDbContext>
    {
        public override PostgreSqlCatalogDbContext CreateDbContext(string[] args)
        {
            return new PostgreSqlCatalogDbContext(GetConfiguration(args), ConnectionStringTemplates.POSTGRESQL, new DbContextOptions<PostgreSqlCatalogDbContext>());
        }
    }
}