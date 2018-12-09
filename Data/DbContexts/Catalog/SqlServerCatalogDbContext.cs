using GoldRush.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Data.DbContexts
{
    public class SqlServerCatalogDbContext : BaseCatalogDbContext<DbContextOptions<SqlServerCatalogDbContext>>
    {
        public SqlServerCatalogDbContext(IConfiguration configuration, DbContextOptions<SqlServerCatalogDbContext> options) : base(configuration, options)
        {
        }

        public SqlServerCatalogDbContext(IConfiguration configuration, string connectionString, DbContextOptions<SqlServerCatalogDbContext> options) : base(configuration, connectionString, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString, options =>
            {
                options.UseRowNumberForPaging(!string.IsNullOrEmpty(edition) && edition.ToUpper().Equals("SQL2008R2"));
                options.MigrationsAssembly(migrationsAssembly);
            });
        }
    }
}