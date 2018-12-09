using System;
using System.Linq;
using GoldRush.Core.Extensions;
using GoldRush.Infrastructure.Security.Identity.Managers;
using GoldRush.Infrastructure.Security.Identity.Stores;
using GoldRush.Infrastructure.Security.Identity.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoldRush.Infrastructure.Extensions
{
    public static class StartupExtensions
    {
        public static IMvcBuilder AddCoreAssemblies(this IMvcBuilder builder, IConfiguration configuration)
        {
            var options = configuration.GetSection("MultitenancyOptions");
            var migrationAssembly = options.GetValue<string>("MigrationAssembly", "Migrations");
            var assemblies = CoreModuleExtensions.GetReferencingAssemblies(typeof(GoldRush.Core.IModule).Namespace); // TODO: Needs enhancements
            var assembly = assemblies.First();
            builder.AddApplicationPart(assembly);

            return builder;
        }

        public static IServiceCollection AddCoreIdentity(this IServiceCollection services)
        {
            services
                .AddAppIdentity()
                .AddUserStore<IdentityUserStore>()
                .AddRoleStore<IdentityRoleStore>()
                .AddUserValidator<IdentityUserValidator>()
                .AddRoleValidator<IdentityRoleValidator>()
                .AddUserManager<IdentityUserManager>()
                .AddRoleManager<IdentityRoleManager>()
                .AddDefaultTokenProviders();

            services.AddScoped<DbContextErrorDescriber>();
            services.AddScoped<IdentityUserStore>();
            services.AddScoped<IdentityRoleStore>();

            return services;
        }

        public static IServiceCollection AddCoreAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAppAuthentication(configuration)
                .AddAppAuthorization();
            return services;
        }

        private static object GetReferencingAssemblies(string v)
        {
            throw new NotImplementedException();
        }
    }
}