using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StructureMap;

namespace GoldRush.Multitenancy
{
    public static class Extensions
    {
        public static Registry AddMultitenancy<TTenant, TResolver>(this Registry services)
            where TTenant : class
            where TResolver : class, ITenantResolver<TTenant>
        {
            services.For<ITenantResolver<TTenant>>().Use<TResolver>().ContainerScoped();
            services.For<IHttpContextAccessor>().Use<HttpContextAccessor>().Singleton();

            services.For<TenantContext<TTenant>>().Use(provider => provider.GetInstance<IHttpContextAccessor>().HttpContext.GetTenantContext<TTenant>()).ContainerScoped();
            services.For<TTenant>().Use(provider => provider.GetInstance<TenantContext<TTenant>>().Tenant).ContainerScoped();
            services.For<TenantWrapper<TTenant>>().Use(provider => new TenantWrapper<TTenant>(provider.GetInstance<TTenant>())).ContainerScoped();
            services.For<ITenant<TTenant>>().Use(provider => provider.GetInstance<TenantWrapper<TTenant>>()).ContainerScoped();

            return services;
        }

        public static IServiceCollection AddMultitenancy<TTenant, TResolver>(this IServiceCollection services)
                where TResolver : class, ITenantResolver<TTenant>
                where TTenant : class
        {
            services.AddTransient<ITenantResolver<TTenant>, TResolver>();

            // No longer registered by default as of ASP.NET Core RC2
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Make Tenant and TenantContext injectable
            // I have to temporarily comment this out due to a bug in disposing TenantContext in a memory cahced resolver
            services.AddScoped(prov => prov.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>());
            services.AddScoped(prov => prov.GetService<TenantContext<TTenant>>()?.Tenant);
            // services.AddScoped(prov => {
            //     var context = prov.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>();
            //     var tenant = context?.Tenant;
            //     return tenant;
            // });

            // Ensure caching is available for caching resolvers
            var resolverType = typeof(TResolver);
            if (typeof(MemoryCacheTenantResolver<TTenant>).IsAssignableFrom(resolverType))
            {
                services.AddMemoryCache();
            }

            return services;
        }

        public static IApplicationBuilder UseMultitenancy<TTenant>(this IApplicationBuilder app) where TTenant : class
        {
            return app.UseMiddleware<TenantResolutionMiddleware<TTenant>>();
        }

        public static IApplicationBuilder UsePerTenant<TTenant>(this IApplicationBuilder app,
            Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration) where TTenant : class
        {
            app.Use(next => new TenantPipelineMiddleware<TTenant>(next, app, configuration).Invoke);
            return app;
        }

        public static void ConfigureTenants<TTenant>(this IContainer container, Action<ConfigurationExpression> configure)
        {
            container.Configure(_ =>
                _.For<ITenantContainerBuilder<TTenant>>()
                    .Use(new TenantContainerBuilder<TTenant>(container, (tenant, config) => configure(config)))
            );
        }

        public static void ConfigureTenants<TTenant>(this IContainer container, Action<TTenant, ConfigurationExpression> configure)
        {
            container.Configure(_ =>
                _.For<ITenantContainerBuilder<TTenant>>()
                    .Use(new TenantContainerBuilder<TTenant>(container, configure))
            );
        }

        public static void ConfigureTenants<TTenant>(this ConfigurationExpression expr, IContainer container, Action<TTenant, ConfigurationExpression> configure)
        {
            expr.For<ITenantContainerBuilder<TTenant>>().Use(new TenantContainerBuilder<TTenant>(container, configure));
        }

        public static IApplicationBuilder UseTenantContainers<TTenant>(this IApplicationBuilder app) where TTenant : class
        {
            return app.UseMiddleware<MultitenantContainerMiddleware<TTenant>>();
        }

        public static IContainer GetTenantContainer<TTenant>(this TenantContext<TTenant> tenantContext) where TTenant : class
        {
            object tenantContainer;

            if (tenantContext.Properties.TryGetValue(CURRENT_TENANT_CONTAINER_KEY, out tenantContainer))
            {
                return tenantContainer as IContainer;
            }
            return null;
        }

        public static void SetTenantContainer<TTenant>(this TenantContext<TTenant> tenantContext, IContainer container) where TTenant : class
        {
            if (tenantContext.Properties.ContainsKey(CURRENT_TENANT_CONTAINER_KEY))
            {
                tenantContext.Properties[CURRENT_TENANT_CONTAINER_KEY] = container;
            }
            else
            {
                tenantContext.Properties.Add(CURRENT_TENANT_CONTAINER_KEY, container);
            }
        }

        public const string CURRENT_TENANT_CONTEXT_KEY = "GoldRush.Multitenancy.CurrentTenantContext";
        public const string CURRENT_TENANT_CONTAINER_KEY = "GoldRush.Multitenancy.CurrentTenantContainer";

        public static TenantContext<TTenant> GetTenantContext<TTenant>(this HttpContext context) where TTenant : class
        {
            object tenantContext;
            if (context.Items.TryGetValue(CURRENT_TENANT_CONTEXT_KEY, out tenantContext))
            {
                return tenantContext as TenantContext<TTenant>;
            }

            return null;
        }

        public static void SetCurrentTenantContext<TTenant>(this HttpContext context, TenantContext<TTenant> tenantContext) where TTenant : class
        {
            if (context.Items.ContainsKey(CURRENT_TENANT_CONTEXT_KEY))
            {
                context.Items[CURRENT_TENANT_CONTEXT_KEY] = tenantContext;
            }
            else
            {
                context.Items.Add(CURRENT_TENANT_CONTEXT_KEY, tenantContext);
            }
        }
    }
}
