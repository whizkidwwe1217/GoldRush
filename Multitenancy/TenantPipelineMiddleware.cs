using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace GoldRush.Multitenancy
{
    public class TenantPipelineMiddleware<TTenant> where TTenant : class
    {
        private readonly RequestDelegate next;
        private readonly IApplicationBuilder rootApp;
        private readonly Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration;

        private readonly ConcurrentDictionary<TTenant, Lazy<RequestDelegate>> pipelines
            = new ConcurrentDictionary<TTenant, Lazy<RequestDelegate>>();

        public TenantPipelineMiddleware(
            RequestDelegate next,
            IApplicationBuilder rootApp,
            Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration)
        {
            this.next = next;
            this.rootApp = rootApp;
            this.configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var tenantContext = context.GetTenantContext<TTenant>();

            if (tenantContext != null && tenantContext.Tenant != null)
            {
                var tenantPipeline = pipelines.GetOrAdd(
                    tenantContext.Tenant,
                    new Lazy<RequestDelegate>(() => BuildTenantPipeline(tenantContext)));

                await tenantPipeline.Value(context);
            }
            else
            {
                await next.Invoke(context);
            }
        }

        private RequestDelegate BuildTenantPipeline(TenantContext<TTenant> tenantContext)
        {
            var branchBuilder = rootApp.New();

            var builderContext = new TenantPipelineBuilderContext<TTenant>
            {
                TenantContext = tenantContext,
                Tenant = tenantContext.Tenant
            };

            configuration(builderContext, branchBuilder);

            // register root pipeline at the end of the tenant branch
            branchBuilder.Run(next);

            return branchBuilder.Build();
        }
    }
}