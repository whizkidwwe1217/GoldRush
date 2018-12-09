using GoldRush.Core.Models;
using GoldRush.Core.Controllers;
using GoldRush.Infrastructure.Services;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GoldRush.Infrastructure.Controllers
{
    public class UserController : BaseServiceController<int, User, IUserService>
    {
        public UserController(IUserService service) : base(service)
        {
        }

        [HttpGet("getuserroles/{userId}")]
        public async Task<List<string>> GetUserRoles(int userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Service.GetUserRoles(userId, cancellationToken);
        }
    }
}