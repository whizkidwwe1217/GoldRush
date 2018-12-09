using GoldRush.Core;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public class PostgreSqlDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<PostgreSqlDbContext>
    {
        public override PostgreSqlDbContext CreateDbContext(string[] args)
        {
            return new PostgreSqlDbContext(GetContainer(), GetConfiguration(args),
                new Tenant
                {
                    ConnectionString = ConnectionStringTemplates.POSTGRESQL,
                    Engine = DatabaseEngine.PostgreSql,
                    IsIsolated = true
                },
                new DbContextOptions<PostgreSqlDbContext>());
        }
    }
}