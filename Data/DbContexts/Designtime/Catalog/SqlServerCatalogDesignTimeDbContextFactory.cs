using Microsoft.EntityFrameworkCore;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public class SqlServerCatalogDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<SqlServerCatalogDbContext>
    {
        public override SqlServerCatalogDbContext CreateDbContext(string[] args)
        {
            return new SqlServerCatalogDbContext(GetConfiguration(args), ConnectionStringTemplates.MSSQL, new DbContextOptions<SqlServerCatalogDbContext>());
        }
    }
}