using GoldRush.Core.Database;
using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StructureMap;

namespace GoldRush.Data.DbContexts
{
    public class PostgreSqlDbContext : BaseDbContext<DbContextOptions<PostgreSqlDbContext>>
    {
        public PostgreSqlDbContext(IConfiguration configuration, Tenant tenant, DbContextOptions<PostgreSqlDbContext> options) : base(configuration, tenant, options)
        {
        }

        public PostgreSqlDbContext(IContainer container, IConfiguration configuration, Tenant tenant, DbContextOptions<PostgreSqlDbContext> options) : base(container, configuration, tenant, options)
        {
        }
    }
}