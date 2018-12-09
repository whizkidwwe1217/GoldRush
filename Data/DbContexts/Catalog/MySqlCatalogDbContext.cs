using GoldRush.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Data.DbContexts
{
    public class MySqlCatalogDbContext : BaseCatalogDbContext<DbContextOptions<MySqlCatalogDbContext>>
    {
        public MySqlCatalogDbContext(IConfiguration configuration, DbContextOptions<MySqlCatalogDbContext> options) : base(configuration, options)
        {
        }

        public MySqlCatalogDbContext(IConfiguration configuration, string connectionString, DbContextOptions<MySqlCatalogDbContext> options) : base(configuration, connectionString, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(this.connectionString, options => {
                options.MigrationsAssembly(migrationsAssembly);
            });
        }
    }
}