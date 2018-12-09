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
    public class BaseStartup
    {
        public BaseStartup(IConfiguration configuration)
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
            ConfigureTenantResolver(services);
            services.AddLogging();
            services.AddCoreIdentity()
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
            services.AddMvc(options => options.Filters.Add<RequestAbortedFilter>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    // options.SerializerSettings.Formatting = Formatting.Indented;
                })
                .AddCoreAssemblies(Configuration);
            ConfigureAdditionalServices(services);
            if (UseSignalR)
                services.AddSignalR();

            if (UseSpa && UseStaticFiles)
            {
                services.AddSpaStaticFiles(config =>
                {
                    config.RootPath = SpaDistPath;
                });
            }

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
            app.UseTenantContainers<Tenant>();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseMiddleware<InvalidCatalogAccessMiddleware>();
            app.UseCors(builder => builder
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                        //.AllowCredentials()
                        );
            // This is used in integration testing
            ConfigureAdditionalMiddleware(app);

            app.UseAuthentication();

            if (UseStaticFiles)
                app.UseStaticFiles();
            if (UseStaticFiles && UseSpa)
                app.UseSpaStaticFiles();
            if (UseSignalR)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<DatabaseMigrationHub>("/api/hubs/dbmigrationhub");
                    ConfigureSignalRHubs(routes);
                });
            }

            app.UseMiddleware<LicenseMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
            if (UseStaticFiles && UseSpa)
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = SpaSourcePath;
                    /* Enable Server-Side Rendering (SSR)              
                        // <!-- In, csproj file, set this to true if you enable server-side prerendering -->
                        // <BuildServerSideRenderer>false</BuildServerSideRenderer>
                        spa.UseSpaPrerendering(options =>
                        {
                            options.BootModulePath = $"{spa.Options.SourcePath}/dist-server/main.bundle.js";
                            options.BootModuleBuilder = env.IsDevelopment() ? new AngularCliBuilder(npmScript: "build:ssr") : null;
                            options.ExcludeUrls = new[] { "/socksjs-node" };
                            options.SupplyData = (context, data) =>
                            {
                                // Creates a new value called isHttpsRequest that's passed to TypeScript code
                                data["isHttpsRequest"] = context.Request.IsHttps;
                            };
                        });
                    */
                    if (env.IsDevelopment())
                    {
                        // To enable calling npm start automatically, use UseAngularCliServer method instead of UseProxyToSpaDevelopmentServer
                        if (UseAngularCliServer && SpaCliServerType == SpaCliServerType.Angular)
                            spa.UseAngularCliServer(npmScript: SpaNpmScript);
                        if (UseReactCliServer && SpaCliServerType == SpaCliServerType.React)
                            spa.UseReactDevelopmentServer(npmScript: SpaNpmScript);

                        if (UseProxyToSpaDevelopmentServer)
                            spa.UseProxyToSpaDevelopmentServer(SpaDevelopmentUri);
                    }
                });
            }
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
