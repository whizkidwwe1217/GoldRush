using System.Linq;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Multitenancy;
using GoldRush.Multitenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure
{
    [Route("api/app")]
    public class ApplicationController : ControllerBase
    {
        private readonly DbContext context;
        private readonly TenantContext<Tenant> tenantContext;

        public ApplicationController(IActiveDbContext activeDbContext, TenantContext<Tenant> tenantContext)
        {
            this.context = activeDbContext.DbContext;
            this.tenantContext = tenantContext;
        }

        [Route("info")]
        public async Task<IActionResult> GetInfo()
        {
            var tenant = tenantContext?.Tenant;
            var hasCompany = false;
            var initialized = true;

            if (tenant != null && tenant.IsIsolated)
            {
                try
                {
                    context.Database.ExecuteSqlCommand("SELECT 1");
                }
                catch
                {
                    initialized = false;
                }
            }

            if (tenant != null && initialized)
            {
                if (tenant.IsIsolated)
                    tenant = await context.Set<Tenant>().Where(e => e.HostName == tenant.HostName).FirstOrDefaultAsync();
                else
                    tenant = await context.Set<Tenant>().FindAsync(tenant.Id);
                if (tenant != null)
                    hasCompany = await context.Set<Company>().AnyAsync(e => e.TenantId == tenant.Id);
                else
                    hasCompany = false;
            }

            return Ok(new
            {
                tenant,
                isAdmin = tenant == null || (bool)tenant?.IsTenantAdministrator,
                isTenant = tenant != null,
                hasCompany
            });
        }
    }
}