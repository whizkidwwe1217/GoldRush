using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Core.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly ICatalogDataSource ds;

        public CatalogRepository(ICatalogDataSource dataSource)
        {
            this.ds = dataSource;
        }

        public async Task InitializeTenant(DbContext context, Tenant tenant,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var found = await context.Set<Tenant>()
                .Where(e => e.Name == tenant.Name && e.HostName == e.HostName)
                .FirstOrDefaultAsync(cancellationToken);

            if (found == null)
            {
                await context.Set<Tenant>().AddAsync(tenant, cancellationToken);
                //await context.SaveChangesAsync();
            }

            var foundId = found != null ? found.Id : tenant.Id;
            var foundName = found != null ? found.Name : tenant.Name;

            var hasCompany = await context.Set<Company>()
                .AnyAsync(c => c.TenantId == foundId && c.Code == foundName, cancellationToken);
            if (!hasCompany)
            {
                var company = new Company
                {
                    Code = tenant.Name,
                    Name = tenant.Name,
                    Tenant = found != null ? found : tenant,
                    DateCreated = DateTime.UtcNow
                };

                await context.Set<Company>().AddAsync(company, cancellationToken);
                //await context.SaveChangesAsync();
                var passwordHasher = new PasswordHasher<User>();
                var user = new User
                {
                    Company = company,
                    Tenant = tenant,
                    Active = true,
                    IsSystemAdministrator = true,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Password = "12345678",
                    NormalizedEmail = "ADMIN@HORDEFLOW.COM",
                    Email = "admin@hordeflow.com",
                    RecoveryEmail = "admin@hordeflow.com",
                    Deleted = false,
                    DateCreated = DateTime.UtcNow,
                    EmailConfirmed = true,
                    IsConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                user.PasswordHash = passwordHasher.HashPassword(user, "12345678");
                await context.Set<User>().AddAsync(user, cancellationToken);

                var role = new Role()
                {
                    Active = true,
                    Name = "Superuser",
                    NormalizedName = "SUPERUSER",
                    Company = company,
                    Tenant = company.Tenant
                };

                await context.Set<Role>().AddAsync(role, cancellationToken);

                var claim = new Claim(CompanyClaimTypes.Role, role.Name);
                var userClaim = new UserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    User = user
                };
                await context.Set<UserClaim>().AddAsync(userClaim, cancellationToken);

                var userRole = new UserRole
                {
                    Role = role,
                    User = user
                };

                await context.Set<UserRole>().AddAsync(userRole, cancellationToken);
            }

            if (context.ChangeTracker.HasChanges())
                await context.SaveChangesAsync(cancellationToken);
        }

        public async Task MigrateAsync(DbContext context, Tenant tenant, DeploymentModes deploymentMode)
        {
            var migrator = new DbContextMigrator(context);

            try
            {
                if (deploymentMode == DeploymentModes.Multi || (tenant.IsIsolated && deploymentMode == DeploymentModes.Hybrid))
                    await migrator.MigrateAsync();

                await InitializeTenant(context, tenant);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException != null ? ex.Message + ": " + ex.InnerException.Message : ex.Message, ex.InnerException != null ? ex.InnerException : ex);
            }
        }

        public async Task DropDatabaseAsync(DbContext context, Tenant tenant, DeploymentModes deploymentMode)
        {
            var migrator = new DbContextMigrator(context);

            try
            {
                if (deploymentMode == DeploymentModes.Multi || (tenant.IsIsolated && deploymentMode == DeploymentModes.Hybrid))
                    await migrator.DropDatabaseAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException != null ? ex.Message + ": " + ex.InnerException.Message : ex.Message, ex.InnerException != null ? ex.InnerException : ex);
            }
        }
    }
}