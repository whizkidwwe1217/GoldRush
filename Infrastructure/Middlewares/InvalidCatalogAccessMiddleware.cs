using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace GoldRush.Infrastructure.Middlewares
{
    public class InvalidCatalogAccessMiddleware
    {
        private readonly RequestDelegate next;

        public InvalidCatalogAccessMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        private bool IsAccessingCatalogPath(string path)
        {
            return path.StartsWith("/api/admin");
        }

        private bool IsNonApiPathOrWhitelisted(string path)
        {
            return path.StartsWith("/api") == false
                || path.StartsWith("/api/app/assembly")
                || path.StartsWith("/api/hubs")
                || path.StartsWith("/api/app");
        }

        public async Task Invoke(HttpContext context)
        {
            var tenant = context.GetTenantContext<Tenant>()?.Tenant;
            if (tenant != null && IsAccessingCatalogPath(context.Request.Path) && !tenant.IsTenantAdministrator)
                context.Response.StatusCode = 404;
            else if (tenant == null && !IsAccessingCatalogPath(context.Request.Path) && !IsNonApiPathOrWhitelisted(context.Request.Path))
            {
                context.Response.StatusCode = 404;
            }
            else
                await this.next.Invoke(context);
        }
    }
}