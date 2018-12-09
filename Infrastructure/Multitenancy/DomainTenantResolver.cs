using System.Threading.Tasks;
using GoldRush.Multitenancy;
using GoldRush.Core.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GoldRush.Core;
using GoldRush.Data.DbContexts;
using Microsoft.Extensions.Configuration;

namespace GoldRush.Infrastructure.Multitenancy
{
    public class DomainTenantResolver : ITenantResolver<Tenant>
    {
        private readonly IConfiguration configuration;
        private readonly ICatalogDataSource datasource;

        public DomainTenantResolver(IConfiguration configuration, ICatalogDataSource datasource)
        {
            this.configuration = configuration;
            this.datasource = datasource;
        }

        public async Task<TenantContext<Tenant>> ResolveAsync(HttpContext context)
        {
            var hostName = context.Request.Host.Value.ToLower();
            var host = context.Request.Host;
            string companyId = context.Request.Headers["CompanyId"].FirstOrDefault();
            var path = context.Request.Path.Value;
            Tenant tenant = null;
            var isWhiteListed = IsPathIsInCatalogWhitelist(path);

            var tenantContext = new TenantContext<Tenant>(tenant);
            tenantContext.Properties.Add("CompanyId", companyId);
            return await Task.FromResult(result: tenantContext);
        }

        private bool IsPathIsInCatalogWhitelist(string path)
        {
            return path == "/api/admin/tenantcatalog/migrate" || path == "/api/admin/tenantcatalog/drop";
        }
    }
}
