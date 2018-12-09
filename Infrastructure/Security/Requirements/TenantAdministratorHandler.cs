using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Infrastructure.Security.Requirements
{
    public class TenantAdministratorHandler : AuthorizationHandler<TenantAdministratorRequirement>
    {
        private readonly IConfiguration configuration;

        public TenantAdministratorHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantAdministratorRequirement requirement)
        {
            if(!context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Issuer == configuration["AdminTokenAuthentication:Issuer"]))
            {
                return Task.CompletedTask;
            }

            var email = context.User.FindFirst(c => c.Type == ClaimTypes.Email && c.Issuer == configuration["AdminTokenAuthentication:Issuer"]).Value;
            if(email != string.Empty && email.Equals("whizkidwwe1217@live.com"))
            {
                context.Succeed(requirement);
            }
            //TODO: Use the following if targeting a version of
            //.NET Framework older than 4.6:
            //      return Task.FromResult(0);
            return Task.CompletedTask;
        }
    }
}