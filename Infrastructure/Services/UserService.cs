using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Core.Services;
using GoldRush.Infrastructure.Repositories;

namespace GoldRush.Infrastructure.Services
{
    public interface IUserService : IService<int, User>
    {
        Task<List<string>> GetUserRoles(int userId, CancellationToken cancellationToken = default(CancellationToken));
    }
    public class UserService : BaseService<int, User, IUserRepository>, IUserService
    {
        public UserService(IUserRepository repository) : base(repository)
        {
        }

        public async Task<List<string>> GetUserRoles(int userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Repository.GetUserRoles(userId, cancellationToken);
        }
    }
}