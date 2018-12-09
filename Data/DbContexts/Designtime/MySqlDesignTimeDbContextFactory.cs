using GoldRush.Core;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public class MySqlDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<MySqlDbContext>
    {
        public override MySqlDbContext CreateDbContext(string[] args)
        {
            return new MySqlDbContext(GetContainer(), GetConfiguration(args),
                new Tenant
                {
                    ConnectionString = ConnectionStringTemplates.MYSQL,
                    Engine = DatabaseEngine.MySql,
                    IsIsolated = true
                },
                new DbContextOptions<MySqlDbContext>());
        }
    }
}