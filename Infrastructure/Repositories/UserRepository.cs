using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Core.Repositories;
using GoldRush.Infrastructure.Security.Identity.Managers;

namespace GoldRush.Infrastructure.Repositories
{
    public interface IUserRepository : IRepository<int, User>
    {
        Task<List<string>> GetUserRoles(int userId, CancellationToken cancellationToken = default(CancellationToken));
    }
    public class UserRepository : BaseCompanyRepository<int, User>, IUserRepository
    {
        private readonly IdentityUserManager manager;

        public UserRepository(IRepositoryManager<int> repositoryManager, IdentityUserManager manager) : base(repositoryManager)
        {
            this.manager = manager;
        }

        public async Task<List<string>> GetUserRoles(int userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await manager.FindByIdAsync(userId);
            var roles = await manager.GetRolesAsync(user);
            return roles.ToList();
        }
    }
}