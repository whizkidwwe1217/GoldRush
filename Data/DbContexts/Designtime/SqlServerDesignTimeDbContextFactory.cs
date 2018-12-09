using GoldRush.Core;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public class SqlServerDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<SqlServerDbContext>
    {
        public override SqlServerDbContext CreateDbContext(string[] args)
        {
            return new SqlServerDbContext(GetContainer(), GetConfiguration(args),
                new Tenant
                {
                    ConnectionString = ConnectionStringTemplates.MSSQL,
                    Engine = DatabaseEngine.SqlServer,
                    IsIsolated = true
                },
                new DbContextOptions<SqlServerDbContext>());
        }
    }
}