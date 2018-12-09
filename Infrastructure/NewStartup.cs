using System;
using GoldRush.Core;
using GoldRush.Multitenancy;
using GoldRush.Infrastructure.Multitenancy;
using GoldRush.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;
using GoldRush.Core.Repositories;
using GoldRush.Core.Services;
using GoldRush.Infrastructure.Extensions;
using GoldRush.Infrastructure.Filters;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using GoldRush.Infrastructure.SignalR;
using Core.Middlewares;
using GoldRush.Infrastructure.Middlewares;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Newtonsoft.Json;
using GoldRush.Core.Audit;

namespace GoldRush.Infrastructure
{
    public class NewStartup
    {
        public NewStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }
        protected virtual void ConfigureTenantResolver(IServiceCollection services)
        {
            services.AddMultitenancy<Tenant, DomainTenantResolver>();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddMultitenancy<Tenant, DomainTenantResolver>()
                .AddLogging()
                .AddCoreIdentity()
                .AddCoreAuthentication(Configuration)
                .AddCors();

            var container = new Container();

            container.Configure(options =>
            {
                options.AddMultitenancy(services, container, Configuration);
                options.Scan(scanner =>
                {
                    scanner.AssembliesFromApplicationBaseDirectory();
                    scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAuditManager<>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IAuditManager<>));
                    scanner.AddAllTypesOf(typeof(IRepositoryManager<>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRepositoryManager<>));
                    scanner.AddAllTypesOf(typeof(IRepository<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRepository<,>));
                    scanner.AddAllTypesOf(typeof(IService<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IService<,>));
                });
            });

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddCoreAssemblies(Configuration);

            container.Populate(services);
            return container.GetInstance<IServiceProvider>();
        }

        protected virtual void ConfigureAdditionalServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                if (UseDeveloperException)
                    app.UseDeveloperExceptionPage();
                else
                    app.UseMiddleware<JsonExceptionMiddleware>();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMultitenancy<Tenant>();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        public virtual SpaCliServerType SpaCliServerType { get; set; } = SpaCliServerType.Angular;

        protected virtual bool UseAngularCliServer
        {
            get { return true; }
        }

        protected virtual bool UseReactCliServer
        {
            get { return false; }
        }

        protected virtual string SpaNpmScript { get; set; } = "start";

        protected virtual bool UseProxyToSpaDevelopmentServer
        {
            get { return true; }
        }

        protected virtual void ConfigureAdditionalMiddleware(IApplicationBuilder app)
        {
        }

        protected virtual bool UseSpa
        {
            get { return true; }
        }

        protected virtual bool UseStaticFiles
        {
            get { return true; }
        }

        protected virtual string SpaDistPath
        {
            get { return "ClientApp/dist"; }
        }

        protected virtual string SpaDevelopmentUri
        {
            get { return "http://localhost:4200"; }
        }

        protected virtual string SpaSourcePath
        {
            get { return "ClientApp"; }
        }

        protected virtual bool UseSignalR
        {
            get { return true; }
        }

        protected virtual bool UseDeveloperException { get { return true; } }

        protected virtual void ConfigureSignalRHubs(HubRouteBuilder routes) { }
    }
}
