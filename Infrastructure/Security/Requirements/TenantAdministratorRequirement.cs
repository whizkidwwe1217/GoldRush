using Microsoft.AspNetCore.Authorization;

namespace GoldRush.Infrastructure.Security.Requirements
{
    public class TenantAdministratorRequirement : IAuthorizationRequirement
    {
        public TenantAdministratorRequirement()
        {
            
        }
    }
}