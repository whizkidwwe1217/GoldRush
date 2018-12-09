using System;
using GoldRush.Core.Extensions;
using GoldRush.Core.Models;
using GoldRush.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StructureMap;

namespace GoldRush.Core.Database
{
    public abstract class BaseDbContext<TDbContextOptions> : DbContext
        where TDbContextOptions : DbContextOptions
    {
        private readonly IContainer container;
        private readonly IConfiguration configuration;
        private Tenant tenant;

        protected BaseDbContext(IConfiguration configuration, Tenant tenant, TDbContextOptions options)
        {
            this.configuration = configuration;
            this.tenant = tenant;
        }

        protected BaseDbContext(IContainer container, IConfiguration configuration, Tenant tenant, TDbContextOptions options)
            : this(configuration, tenant, options)
        {
            this.container = container;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            if (tenant == null)
                this.tenant = new Tenant();

            var connectionString = this.tenant.ConnectionString;
            var resolvedTenant = this.tenant;
            var deploymentMode = DeploymentModes.Multi;
            var catalogEngine = DatabaseEngine.SqlServer;
            var migrationAssembly = "Migrations";
            var edition = string.Empty;

            if (configuration != null)
            {
                var options = configuration.GetSection("MultitenancyOptions");
                catalogEngine = options.GetValue<DatabaseEngine>("CatalogEngine");
                deploymentMode = options.GetValue<DeploymentModes>("DeploymentMode");
                migrationAssembly = options.GetValue<string>("MigrationAssembly", "Migrations");
                connectionString = configuration.GetConnectionString(options.GetValue<string>("ConnectionString"));
                edition = options.GetValue<string>("Edition");
            }

            if (deploymentMode == DeploymentModes.Single)
            {
                resolvedTenant.ConnectionString = connectionString;
                resolvedTenant.Engine = catalogEngine;
                resolvedTenant.Edition = edition;
            }
            else if (deploymentMode == DeploymentModes.Hybrid)
            {
                if (resolvedTenant.IsIsolated)
                {
                    resolvedTenant.Engine = this.tenant.Engine;
                    resolvedTenant.ConnectionString = this.tenant.ConnectionString;
                    resolvedTenant.Edition = this.tenant.Edition;
                }
                else
                {
                    resolvedTenant.Engine = catalogEngine;
                    resolvedTenant.ConnectionString = connectionString;
                    resolvedTenant.Edition = edition;
                }
            }
            else
            {
                if (this.tenant != null)
                {
                    resolvedTenant.Engine = tenant.Engine;
                    resolvedTenant.ConnectionString = tenant.ConnectionString;
                    resolvedTenant.Edition = tenant.Edition;
                }
                else
                {
                    resolvedTenant = new Tenant();
                    resolvedTenant.Engine = catalogEngine;
                    resolvedTenant.ConnectionString = connectionString;
                    resolvedTenant.Edition = edition;
                }
            }

            builder.ResolveDbContextProvider(resolvedTenant.Engine, resolvedTenant.ConnectionString, migrationAssembly, resolvedTenant.Edition);
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var options = configuration.GetSection("MultitenancyOptions");
            var migrationAssembly = options.GetValue<string>("MigrationAssembly", "Migrations");
            var useRowVersion = tenant != null && tenant.Engine == DatabaseEngine.SqlServer;
            builder.ApplyDesignTimeConfigurations(AppDomain.CurrentDomain.BaseDirectory, useRowVersion);
            // This has side-effects where the tenant is always null due to the tenant resolver is the that calls OnModelCreating first. The tenant id is null.
            // Another solution is just to filter using the BaseRepository
            // builder.ApplyGlobalFiltersFromAssembly(this, tenant, AppDomain.CurrentDomain.BaseDirectory);
            base.OnModelCreating(builder);
        }

        public void SetGlobalQuery<TEntity>(ModelBuilder builder, Tenant tenant) where TEntity : class, ITenantEntity<Guid, int>, ICanSoftDelete, ICanActivate
        {
            builder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenant.Id && e.Active && !e.Deleted);
        }
    }
}