using GoldRush.Core;
using GoldRush.Data.DbContexts;
using GoldRush.Core.Models;
using GoldRush.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace GoldRush.Infrastructure.Multitenancy
{
    public static class MultitenancyExtensions
    {
        public static ConfigurationExpression ResolveDbContext(this ConfigurationExpression expression, DatabaseEngine engine)
        {
            switch (engine)
            {
                case DatabaseEngine.SqlServer:
                    expression.For(typeof(DbContext)).Use(typeof(SqlServerDbContext));
                    break;
                case DatabaseEngine.MySql:
                    expression.For(typeof(DbContext)).Use(typeof(MySqlDbContext));
                    break;
                case DatabaseEngine.PostgreSql:
                    expression.For(typeof(DbContext)).Use(typeof(PostgreSqlDbContext));
                    break;
            }

            return expression;
        }

        public static ConfigurationExpression ResolveCatalogDataSource(this ConfigurationExpression expression, DatabaseEngine engine, IConfiguration configuration)
        {
            switch (engine)
            {
                case DatabaseEngine.SqlServer:
                    expression.For<ICatalogDataSource>().Use(new CatalogDataSource(
                            new SqlServerCatalogDbContext(configuration, new DbContextOptions<SqlServerCatalogDbContext>())));
                    break;
                case DatabaseEngine.MySql:
                    expression.For<ICatalogDataSource>()
                        .Use(new CatalogDataSource(
                            new MySqlCatalogDbContext(configuration, new DbContextOptions<MySqlCatalogDbContext>())));
                    break;
                case DatabaseEngine.PostgreSql:
                    expression.For<ICatalogDataSource>()
                        .Use(new CatalogDataSource(
                            new PostgreSqlCatalogDbContext(configuration, new DbContextOptions<PostgreSqlCatalogDbContext>())));
                    break;
            }

            return expression;
        }

        public static ConfigurationExpression ResolveHybridDataSource(this ConfigurationExpression expression, DatabaseEngine engine, IConfiguration configuration)
        {
            var options = configuration.GetSection("MultitenancyOptions");
            var connectionString = configuration.GetConnectionString(options.GetValue<string>("ConnectionString"));
            var tenant = new Tenant { ConnectionString = connectionString, Engine = engine };

            switch (engine)
            {
                case DatabaseEngine.SqlServer:
                    expression.For<ICatalogDataSource>().Use(new CatalogDataSource(
                            new SqlServerDbContext(configuration, tenant, new DbContextOptions<SqlServerDbContext>())));
                    break;
                case DatabaseEngine.MySql:
                    expression.For<ICatalogDataSource>()
                        .Use(new CatalogDataSource(
                            new MySqlDbContext(configuration, tenant, new DbContextOptions<MySqlDbContext>())));
                    break;
                case DatabaseEngine.PostgreSql:
                    expression.For<ICatalogDataSource>()
                        .Use(new CatalogDataSource(
                            new PostgreSqlDbContext(configuration, tenant, new DbContextOptions<PostgreSqlDbContext>())));
                    break;
            }

            return expression;
        }

        public static ConfigurationExpression ConfigureDbContexts(this ConfigurationExpression expression, Tenant tenant, IConfiguration configuration)
        {
            var catalogEngine = configuration.GetSection("MultitenancyOptions").GetValue<DatabaseEngine>("CatalogEngine");
            var deploymentMode = configuration.GetSection("MultitenancyOptions").GetValue<DeploymentModes>("DeploymentMode");

            if (deploymentMode == DeploymentModes.Multi)
            {
                if (tenant != null)
                {
                    expression.ResolveDbContext(tenant.Engine);
                }
                expression.ResolveCatalogDataSource(catalogEngine, configuration);
            }
            else if (deploymentMode == DeploymentModes.Single)
            {
                expression.ResolveDbContext(catalogEngine);
                expression.ResolveHybridDataSource(catalogEngine, configuration);
                // expression.For<ICatalogDataSource>().Use<CatalogDataSource>();
                // expression.For<ICatalogDataSource>().Use(provider => new CatalogDataSource(provider.GetInstance<DbContext>())).ContainerScoped();
            }
            else if (deploymentMode == DeploymentModes.Hybrid)
            {
                var engine = catalogEngine;

                if (tenant != null)
                {
                    if (tenant.IsIsolated)
                    {
                        engine = tenant.Engine;
                        expression.ResolveDbContext(engine);
                    }
                    else
                    {
                        expression.ResolveDbContext(engine);
                    }
                }

                expression.ResolveHybridDataSource(catalogEngine, configuration);
            }

            return expression;
        }

        public static ConfigurationExpression ConfigureCatalogDbContexts(this ConfigurationExpression expression, IConfiguration configuration)
        {
            var catalogEngine = configuration.GetSection("MultitenancyOptions").GetValue<DatabaseEngine>("CatalogEngine");
            var deploymentMode = configuration.GetSection("MultitenancyOptions").GetValue<DeploymentModes>("DeploymentMode");

            if (deploymentMode == DeploymentModes.Multi)
            {
                expression.ResolveCatalogDataSource(catalogEngine, configuration);
            }
            else if (deploymentMode == DeploymentModes.Single)
            {
                expression.ResolveDbContext(catalogEngine);
                expression.For<ICatalogDataSource>().Use<CatalogDataSource>();
            }
            else if (deploymentMode == DeploymentModes.Hybrid)
            {
                expression.ResolveHybridDataSource(catalogEngine, configuration);
            }

            return expression;
        }

        public static ConfigurationExpression AddMultitenancy(
            this ConfigurationExpression expression,
            IServiceCollection services, IContainer container,
            IConfiguration configuration)
        {
            //expression.AddMultitenancy<Tenant, CachedDomainTenantResolver>();
            expression.ConfigureCatalogDbContexts(configuration);
            expression.ConfigureTenants<Tenant>(container, (tenant, cfg) =>
            {
                cfg.ConfigureDbContexts(tenant, configuration);

                if (tenant != null)
                {
                    cfg.For<IActiveDbContext>().Use<TenantActiveDbContext>();
                }
                else
                {
                    cfg.For<IActiveDbContext>().Use<CatalogActiveDbContext>();
                }
            });
            return expression;
        }
    }
}
