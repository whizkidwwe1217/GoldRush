using GoldRush.Core.Database;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StructureMap;

namespace GoldRush.Data.DbContexts
{
    public class MySqlDbContext : BaseDbContext<DbContextOptions<MySqlDbContext>>
    {
        public MySqlDbContext(IConfiguration configuration, Tenant tenant, DbContextOptions<MySqlDbContext> options) : base(configuration, tenant, options)
        {
        }

        public MySqlDbContext(IContainer container, IConfiguration configuration, Tenant tenant, DbContextOptions<MySqlDbContext> options) : base(container, configuration, tenant, options)
        {
        }
    }
}