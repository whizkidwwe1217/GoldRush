using Microsoft.AspNetCore.Authorization;

namespace GoldRush.Infrastructure.Security.Requirements
{
    /**
        How to use
        1. Register to services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Over21",
                                policy => policy.Requirements.Add(new MinimumAgeRequirement(21)));
            });

            services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
        }
        2. Assign to controller
        [Authorize(Policy="Over21")]
        public class AlcoholPurchaseRequirementsController : Controller
        {
            public ActionResult Login()
            {
            }

            public ActionResult Logout()
            {
            }
        }
     */
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int MinimumAge { get; private set; }

        public MinimumAgeRequirement(int minimumAge)
        {
            MinimumAge = minimumAge;
        }
    }
}