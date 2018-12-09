using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GoldRush.Infrastructure.Multitenancy
{
    public class CachedDomainTenantResolver : MemoryCacheTenantResolver<Tenant>
    {
        private readonly ICatalogDataSource datasource;
        public CachedDomainTenantResolver(ICatalogDataSource datasource, IMemoryCache cache, ILoggerFactory loggerFactory) : base(cache, loggerFactory)
        {
            this.datasource = datasource ?? throw new System.ArgumentNullException(nameof(datasource));
        }

        private bool IsPathIsInCatalogWhitelist(string path)
        {
            return path == "/api/admin/tenantcatalog/migrate" || path == "/api/admin/tenantcatalog/drop";
        }

        protected override string GetContextIdentifier(HttpContext context)
        {
            return context.Request.Host.Value.ToLower();
        }

        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<Tenant> context)
        {
            string[] identifiers = new string[0];
            if (context.Tenant == null)
                return new string[0];
            return new string[] { context.Tenant.HostName };
        }

        protected override async Task<TenantContext<Tenant>> ResolveAsync(HttpContext context)
        {
            var hostName = context.Request.Host.Value.ToLower();
            var host = context.Request.Host;
            string companyId = context.Request.Headers["CompanyId"].FirstOrDefault();
            var path = context.Request.Path.Value;
            Tenant tenant = null;
            if (!IsPathIsInCatalogWhitelist(path))
            {
                if (datasource != null && datasource.DbContext != null)
                {
                    try
                    {
                        tenant = await datasource.DbContext.Set<Tenant>()
                            .Where(e => e.HostName == hostName && e.Active && !e.Deleted)
                            .FirstOrDefaultAsync();
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex.Message);
                    }
                }
            }
            var tenantContext = new TenantContext<Tenant>(tenant);
            tenantContext.Properties.Add("CompanyId", companyId);
            return await Task.FromResult(result: tenantContext);
        }
    }
}