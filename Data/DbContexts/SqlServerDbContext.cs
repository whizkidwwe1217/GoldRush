using GoldRush.Core.Database;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StructureMap;

namespace GoldRush.Data.DbContexts
{
    public class SqlServerDbContext : BaseDbContext<DbContextOptions<SqlServerDbContext>>
    {
        public SqlServerDbContext(IConfiguration configuration, Tenant tenant, DbContextOptions<SqlServerDbContext> options) : base(configuration, tenant, options)
        {
        }

        public SqlServerDbContext(IContainer container, IConfiguration configuration, Tenant tenant, DbContextOptions<SqlServerDbContext> options) : base(container, configuration, tenant, options)
        {
        }
    }
}