using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Core.Services;
using GoldRush.Infrastructure.Repositories;

namespace GoldRush.Infrastructure.Services
{
    public interface IRoleService : IService<int, Role> { 
        Task<SearchResponseData> GetMembersAsync(int roleId, SearchParams parameters, CancellationToken cancellationToken = default(CancellationToken));
        Task<UserRole> FindRoleAsync(UserRole userRole);
        Task<UserRole> AddMemberAsync(UserRole userRole, CancellationToken cancellationToken = default(CancellationToken));
    }
    public class RoleService : BaseService<int, Role, IRoleRepository>, IRoleService
    {
        public RoleService(IRoleRepository repository) : base(repository)
        {
        }

        public async Task<UserRole> AddMemberAsync(UserRole userRole, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.AddMemberAsync(userRole, cancellationToken);
        }

        public async Task<UserRole> FindRoleAsync(UserRole userRole)
        {
            return await Repository.FindRoleAsync(userRole);
        }

        public async Task<SearchResponseData> GetMembersAsync(int roleId, SearchParams parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.GetMembersAsync(roleId, parameters, cancellationToken);
        }
    }
}