using System;
using GoldRush.Core.Audit;
using GoldRush.Core.Extensions;
using GoldRush.Core.Models;
using GoldRush.Core.Models.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Core.Database
{
    public abstract class BaseCatalogDbContext<TDbContextOptions> : DbContext
        where TDbContextOptions : DbContextOptions
    {
        protected readonly IConfiguration configuration;
        protected readonly string connectionString;
        protected readonly string migrationsAssembly;
        protected readonly string edition;

        protected BaseCatalogDbContext(IConfiguration configuration, TDbContextOptions options) : base(options)
        {
            this.configuration = configuration;
            var multitenancyOptions = configuration.GetSection("MultitenancyOptions");
            this.connectionString = configuration.GetConnectionString(multitenancyOptions.GetValue<string>("ConnectionString"));
            this.edition = multitenancyOptions.GetValue<string>("Edition");
            this.migrationsAssembly = multitenancyOptions.GetValue<string>("MigrationAssembly", "Migrations");
        }

        protected BaseCatalogDbContext(IConfiguration configuration, string connectionString, TDbContextOptions options)
            : this(configuration, options)
        {
            var multitenancyOptions = configuration.GetSection("MultitenancyOptions");
            this.connectionString = connectionString;
            this.edition = multitenancyOptions.GetValue<string>("Edition");
            this.migrationsAssembly = multitenancyOptions.GetValue<string>("MigrationAssembly", "Migrations");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var options = configuration.GetSection("MultitenancyOptions");
            var catalogEngine = options.GetValue<DatabaseEngine>("CatalogEngine");
            var deploymentMode = options.GetValue<DeploymentModes>("DeploymentMode");
            var useRowVersion = catalogEngine == DatabaseEngine.SqlServer;
            if (deploymentMode == DeploymentModes.Single || deploymentMode == DeploymentModes.Hybrid)
                builder.ApplyDesignTimeConfigurations(AppDomain.CurrentDomain.BaseDirectory, useRowVersion);

            builder.ApplyConfiguration(new TenantConfiguration(useRowVersion));
            builder.ApplyConfiguration(new CompanyConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserConfiguration(useRowVersion));
            builder.ApplyConfiguration(new RoleConfiguration(useRowVersion));
            builder.ApplyConfiguration(new RoleClaimConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserRoleConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserLoginConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserTokenConfiguration(useRowVersion));
            builder.ApplyConfiguration(new UserClaimConfiguration(useRowVersion));
            builder.ApplyConfiguration(new SubscriptionConfiguration(useRowVersion));
            builder.ApplyConfiguration(new AuditEntityConfiguration(useRowVersion));

            base.OnModelCreating(builder);
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
    }
}