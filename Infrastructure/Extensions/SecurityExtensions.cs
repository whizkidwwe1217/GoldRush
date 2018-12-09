using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GoldRush.Infrastructure.Extensions
{
    public static class SecurityExtensions
    {
        public static IdentityBuilder AddAppIdentity(this IServiceCollection services)
        {
            return services.AddIdentity<User, Role>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 2;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Signin settings
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // User settings
                options.User.RequireUniqueEmail = true;
            });
        }
        
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // .AddJwtBearer(cfg => {
            //     cfg.RequireHttpsMetadata = false;
            //     cfg.SaveToken = true;
            //     var signingKeys = new List<SecurityKey>();
            //     if(!string.IsNullOrEmpty(Configuration["AdminTokenAuthentication:SecretKey"])) {
            //         signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AdminTokenAuthentication:SecretKey"])));
            //     }
            //     cfg.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         ValidIssuer = Configuration["AdminTokenAuthentication:Issuer"],
            //         ValidAudience = Configuration["AdminTokenAuthentication:Audience"],
            //         IssuerSigningKeys = signingKeys,
            //         ClockSkew = TimeSpan.Zero,
            //         ValidateAudience = true,
            //         ValidateLifetime = true,
            //         RequireExpirationTime = true,
            //         RequireSignedTokens = true,
            //         ValidateIssuerSigningKey = true,
            //         ValidateIssuer = true
            //     };
            //     cfg.Events = new JwtBearerEvents
            //     {
            //         OnMessageReceived = context => 
            //         {
            //             var accessToken = context.Request.Query["access_token"];

            //             if(!string.IsNullOrEmpty(accessToken) )
            //             {
            //                 context.Token = context.Request.Query["access_token"];
            //             }

            //             return Task.CompletedTask;
            //         }
            //     };
            // })
            // .AddJwtBearer(cfg => {
            //     cfg.RequireHttpsMetadata = false;
            //     cfg.SaveToken = true;
            //     var signingKeys = new List<SecurityKey>();
            //     if(!string.IsNullOrEmpty(Configuration["TokenAuthentication:SecretKey"])) {
            //         signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:SecretKey"])));
            //     }
            //     cfg.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         ValidIssuer = Configuration["TokenAuthentication:Issuer"],
            //         ValidAudience = Configuration["TokenAuthentication:Audience"],
            //         IssuerSigningKeys = signingKeys,
            //         ClockSkew = TimeSpan.Zero,
            //         ValidateAudience = true,
            //         ValidateLifetime = true,
            //         RequireExpirationTime = true,
            //         RequireSignedTokens = true,
            //         ValidateIssuerSigningKey = true,
            //         ValidateIssuer = true
            //     };
            //     cfg.Events = new JwtBearerEvents
            //     {
            //         OnMessageReceived = context => 
            //         {
            //             var accessToken = context.Request.Query["access_token"];

            //             if(!string.IsNullOrEmpty(accessToken) )
            //             {
            //                 context.Token = context.Request.Query["access_token"];
            //             }

            //             return Task.CompletedTask;
            //         }
            //     };
            // });
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;

                var signingKeys = new List<SecurityKey>();

                if(!string.IsNullOrEmpty(Configuration["TokenAuthentication:SecretKey"])) {
                    signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:SecretKey"])));
                }
                if(!string.IsNullOrEmpty(Configuration["AdminTokenAuthentication:SecretKey"])) {
                    signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AdminTokenAuthentication:SecretKey"])));
                }

                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuers = new List<string>()
                    {
                        Configuration["TokenAuthentication:Issuer"],
                        Configuration["AdminTokenAuthentication:Issuer"]
                    },
                    ValidAudiences = new List<string>()
                    {
                        Configuration["TokenAuthentication:Audience"],
                        Configuration["AdminTokenAuthentication:Audience"]
                    },
                    // IssuerSigningKeys = new List<SecurityKey>()
                    // {
                    //     new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:SecretKey"])),
                    //     new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AdminTokenAuthentication:SecretKey"]))
                    // },
                    IssuerSigningKeys = signingKeys,
                    ClockSkew = TimeSpan.Zero,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                };

                cfg.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context => 
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if(!string.IsNullOrEmpty(accessToken) )
                        {
                            context.Token = context.Request.Query["access_token"];
                        }

                        return Task.CompletedTask;
                    }
                };

                //cfg.ClaimsIssuer = Configuration["TokenAuthentication:Issuer"];
                //cfg.Audience = Configuration["TokenAuthentication:Audience"];
            });

            return services;
        }

        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
                //options.AddPolicy("CanSearch", policy => policy.RequireClaim(CompanyClaimTypes.Permission, "CanSearch"));
                options.AddPolicy("TenantAdministrator", policy => 
                    policy.Requirements.Add(new TenantAdministratorRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, TenantAdministratorHandler>();
            return services;
        }
    }
}