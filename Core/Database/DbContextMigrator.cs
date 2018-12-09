using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoldRush.Core
{
    public class DbContextMigrator : IDbContextMigrator
    {        
        private DbContext context;
        private readonly bool ensureDeleted;

        public DbContextMigrator(DbContext context, bool ensureDeleted = false)
            : this(context)
        {
            this.ensureDeleted = ensureDeleted;
        }

        public DbContextMigrator(DbContext context)
        {
            this.context = context;
        }

        public DbContext Context { get => context; set => context = value; }

        public void Migrate()
        {
            if(ensureDeleted)
                context.Database.EnsureDeleted();
            if(!AllMigrationsApplied())
                context.Database.Migrate();
            else
                context.Database.EnsureCreated();
        }

        public async Task MigrateAsync()
        {
            if(ensureDeleted)
                await context.Database.EnsureDeletedAsync();
            if(!AllMigrationsApplied())
                await context.Database.MigrateAsync();
            else
                await context.Database.EnsureCreatedAsync();
        }

        public void DropDatabase()
        {
            context.Database.EnsureDeleted();
        }

        public async Task DropDatabaseAsync()
        {
            await context.Database.EnsureDeletedAsync();
        }

        public bool AllMigrationsApplied()
        {
            var applied = context.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public IEnumerable<string> GetAppliedMigrations()
        {
            var migrations = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(e => e.Key);
            return migrations.ToList();
        }
    }
}