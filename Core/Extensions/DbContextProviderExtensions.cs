using GoldRush.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Core.Extensions
{
    public static class DbContextProviderExtensions
    {
        public static DbContextOptionsBuilder ResolveDbContextProvider(this DbContextOptionsBuilder builder, DatabaseEngine engine, string connectionString, string migrationAssembly, string edition)
        {
            switch (engine)
            {
                case DatabaseEngine.SqlServer:
                    builder.UseSqlServer(connectionString, options =>
                    {
                        options.UseRowNumberForPaging(!string.IsNullOrEmpty(edition) && edition.ToUpper().Equals("SQL2008R2"));
                        options.MigrationsAssembly(migrationAssembly);
                    });
                    break;
                case DatabaseEngine.MySql:
                    builder.UseMySql(connectionString, options =>
                    {
                        options.MigrationsAssembly(migrationAssembly);
                    });
                    break;
                case DatabaseEngine.PostgreSql:
                    builder.UseNpgsql(connectionString, options =>
                    {
                        options.MigrationsAssembly(migrationAssembly);
                    });
                    break;
            }

            return builder;
        }

        public static DbContextOptionsBuilder ConfigureDbContextProvider(this DbContextOptionsBuilder builder, Tenant tenant, IConfiguration configuration)
        {
            var options = configuration.GetSection("MultitenancyOptions");
            var catalogEngine = options.GetValue<DatabaseEngine>("CatalogEngine");
            var edition = options.GetValue<string>("Edition");
            var deploymentMode = options.GetValue<DeploymentModes>("DeploymentMode");
            var connectionString = configuration.GetConnectionString(options.GetValue<string>("ConnectionString"));
            var migrationAssembly = options.GetValue<string>("MigrationAssembly", "Migrations");

            if (deploymentMode == DeploymentModes.Multi)
            {
                if (tenant != null)
                {
                    builder.ResolveDbContextProvider(tenant.Engine, tenant.ConnectionString, migrationAssembly, tenant.Edition);
                }
                builder.ResolveDbContextProvider(catalogEngine, connectionString, migrationAssembly, edition);
            }
            else if (deploymentMode == DeploymentModes.Single)
            {
                builder.ResolveDbContextProvider(catalogEngine, connectionString, migrationAssembly, edition);
            }
            else if (deploymentMode == DeploymentModes.Hybrid)
            {
                var engine = catalogEngine;

                if (tenant != null)
                {
                    if (tenant.IsIsolated)
                    {
                        engine = tenant.Engine;
                        builder.ResolveDbContextProvider(engine, tenant.ConnectionString, migrationAssembly, tenant.Edition);
                    }
                    else
                    {
                        builder.ResolveDbContextProvider(engine, connectionString, migrationAssembly, edition);
                    }
                }
                builder.ResolveDbContextProvider(catalogEngine, connectionString, migrationAssembly, edition);
            }

            return builder;
        }
    }
}