using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Core.Repositories;
using GoldRush.Core.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure.Repositories
{
    public interface ITenantRepository : IRepository<Guid, Tenant>
    {
        Task<SearchResponseData<Company>> GetCompanies(SearchParams parameters,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<Company> GetCompany(Guid id,
            CancellationToken cancellationToken = default(CancellationToken)); 
        Task Initialize(Tenant tenant, CancellationToken cancellationToken = default(CancellationToken));
    }
    public class TenantRepository : BaseRepository<Guid, Tenant>, ITenantRepository
    {
        private readonly ICompanyRepository companyRepository;
        private readonly ITenantDbContextMigrator migrator;

        public TenantRepository(ICompanyRepository companyRepository, IRepositoryManager<Guid> repositoryManager, ITenantDbContextMigrator migrator)
            : base(repositoryManager)
        {
            this.companyRepository = companyRepository;
            this.migrator = migrator;
        }

        public async Task<SearchResponseData<Company>> GetCompanies(SearchParams parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            var companies = await companyRepository.SearchAsync(parameters, cancellationToken);
            return companies;
        }

        public async Task<Company> GetCompany(Guid id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var company = await companyRepository.GetAsync(id, cancellationToken);
            return company;
        }

        public async Task Initialize(Tenant tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(tenant.IsIsolated)
            {
                try
                {
                    await DbContext.Database.ExecuteSqlCommandAsync("SELECT 1");
                }
                catch
                {
                    await migrator.MigrateAsync();    
                }
            }

            var found = await DbContext.Set<Tenant>()
                .Where(e => e.Name == tenant.Name && e.HostName == e.HostName)
                .FirstOrDefaultAsync(cancellationToken);

            if (found == null)
            {
                await DbContext.Set<Tenant>().AddAsync(tenant, cancellationToken);
                //await DbContext.SaveChangesAsync();
            }

            var foundId = found != null ? found.Id : tenant.Id;
            var foundName = found != null ? found.Name : tenant.Name;

            var hasCompany = await DbContext.Set<Company>()
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

                await DbContext.Set<Company>().AddAsync(company, cancellationToken);
                //await DbContext.SaveChangesAsync();
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
                await DbContext.Set<User>().AddAsync(user, cancellationToken);

                var role = new Role()
                {
                    Active = true,
                    Name = "Superuser",
                    NormalizedName = "SUPERUSER",
                    Company = company,
                    Tenant = company.Tenant
                };

                await DbContext.Set<Role>().AddAsync(role, cancellationToken);

                var claim = new Claim(CompanyClaimTypes.Role, role.Name);
                var userClaim = new UserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    User = user
                };
                 await DbContext.Set<UserClaim>().AddAsync(userClaim, cancellationToken);

                var userRole = new UserRole
                {
                    Role = role,
                    User = user
                };

                await DbContext.Set<UserRole>().AddAsync(userRole, cancellationToken);
            }

            if (DbContext.ChangeTracker.HasChanges())
                await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}