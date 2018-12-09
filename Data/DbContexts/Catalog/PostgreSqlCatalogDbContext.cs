using GoldRush.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Data.DbContexts
{
    public class PostgreSqlCatalogDbContext : BaseCatalogDbContext<DbContextOptions<PostgreSqlCatalogDbContext>>
    {
        public PostgreSqlCatalogDbContext(IConfiguration configuration, DbContextOptions<PostgreSqlCatalogDbContext> options) : base(configuration, options)
        {
        }

        public PostgreSqlCatalogDbContext(IConfiguration configuration, string connectionString, DbContextOptions<PostgreSqlCatalogDbContext> options) : base(configuration, connectionString, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString, options => {
                options.MigrationsAssembly(migrationsAssembly);
            });
        }
    }
}