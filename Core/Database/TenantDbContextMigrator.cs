using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoldRush.Core
{
    public class TenantDbContextMigrator : ITenantDbContextMigrator
    {
        private readonly bool ensureDeleted;
        private DbContext context;

        public TenantDbContextMigrator(DbContext context, bool ensureDeleted = false)
            : this(context)
        {
            this.ensureDeleted = ensureDeleted;
        }

        public TenantDbContextMigrator(DbContext context)
        {
            this.context = context;
        }

        public DbContext DbContext { get => context; set => context = value; }

        public void Migrate()
        {
            if (ensureDeleted)
                DbContext.Database.EnsureDeleted();
            if (!AllMigrationsApplied())
                DbContext.Database.Migrate();
            else
                DbContext.Database.EnsureCreated();
        }

        public async Task MigrateAsync()
        {
            if (ensureDeleted)
                await DbContext.Database.EnsureDeletedAsync();
            if (!AllMigrationsApplied())
                await DbContext.Database.MigrateAsync();
            else
                await DbContext.Database.EnsureCreatedAsync();
        }

        public void DropDatabase()
        {
            DbContext.Database.EnsureDeleted();
        }

        public async Task DropDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
        }

        public bool AllMigrationsApplied()
        {
            var applied = DbContext.GetService<IHistoryRepository>()?
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = DbContext.GetService<IMigrationsAssembly>()?
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public IEnumerable<string> GetAllMigrations()
        {
            var migrations = DbContext.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(e => e.Key);
            return migrations.ToList();
        }

        public IEnumerable<string> GetAppliedMigrations()
        {
            var applied = DbContext.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);
            return applied.ToList();
        }

        public IEnumerable<object> GetMigrations()
        {
            var applied = DbContext.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var all = DbContext.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            var migrations = applied.FullOuterJoin(all, a => a, b => b, (a, b, id) => new { Id = a == null ? b : a, Applied = a == null ? false : true });
            return migrations.ToList();
        }
    }
}