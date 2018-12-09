using Microsoft.EntityFrameworkCore;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public class MySqlCatalogDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<MySqlCatalogDbContext>
    {
        public override MySqlCatalogDbContext CreateDbContext(string[] args)
        {
            return new MySqlCatalogDbContext(GetConfiguration(args), ConnectionStringTemplates.MYSQL, new DbContextOptions<MySqlCatalogDbContext>());
        }
    }
}