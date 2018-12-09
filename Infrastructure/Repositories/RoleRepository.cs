using System;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Core.Repositories;
using GoldRush.Infrastructure.Security.Identity.Managers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GoldRush.Infrastructure.Repositories
{
    public interface IRoleRepository : IRepository<int, Role> {
        Task<SearchResponseData> GetMembersAsync(int roleId, SearchParams parameters, CancellationToken cancellationToken = default(CancellationToken));
        Task<UserRole> FindRoleAsync(UserRole userRole);
        Task<UserRole> AddMemberAsync(UserRole userRole, CancellationToken cancellationToken = default(CancellationToken));
    }
    public class RoleRepository : BaseCompanyRepository<int, Role>, IRoleRepository
    {
        private readonly IdentityRoleManager roleManager;

        public RoleRepository(IRepositoryManager<int> repositoryManager, IdentityRoleManager roleManager) : base(repositoryManager)
        {
            this.roleManager = roleManager;
        }

        public async Task<SearchResponseData> GetMembersAsync(int roleId, SearchParams parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var role = await roleManager.FindByIdAsync(roleId.ToString());
            if (role != null)
            {
                var members = from userRoles in DbContext.Set<UserRole>()
                              join users in DbContext.Set<User>() on userRoles.UserId equals users.Id
                              where userRoles.RoleId == roleId && userRoles.UserId == users.Id
                              select users;
                return await members
                    .Search<User, int>(parameters)
                    .ToListAsync<User>(parameters, cancellationToken);
            }
            return null;
        }

        public async Task<UserRole> FindRoleAsync(UserRole userRole)
        {
            var found = await DbContext.Set<UserRole>().Where(e => e.UserId == userRole.UserId && e.RoleId == userRole.RoleId).FirstOrDefaultAsync();
            if (found != null)
            {
                throw new Exception("Member already exists.");
            }
            return found;
        }

        public async Task<UserRole> AddMemberAsync(UserRole userRole, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DbContext.Set<UserRole>().AddAsync(userRole, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            return await Task.FromResult(userRole);
        }
    }
}