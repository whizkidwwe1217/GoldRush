using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using StructureMap;

namespace GoldRush.Data.DbContexts.DesignTime
{
    public abstract class BaseDesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        protected IConfiguration GetConfiguration(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            return config;
        }

        protected IContainer GetContainer()
        {
            var container = new Container();
            container.Configure(c =>
            {
                c.Scan(x =>
                {
                    x.AssembliesFromApplicationBaseDirectory();
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });
            });
            return container;
        }
        
        public abstract TContext CreateDbContext(string[] args);
    }
}