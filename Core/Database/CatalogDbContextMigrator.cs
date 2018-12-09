using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoldRush.Core
{
    public class CatalogDbContextMigrator : ICatalogDbContextMigrator
    {
        private ICatalogDataSource dataSource;
        private readonly bool ensureDeleted;

        public CatalogDbContextMigrator(ICatalogDataSource dataSource, bool ensureDeleted = false)
            : this(dataSource)
        {
            this.ensureDeleted = ensureDeleted;
        }

        public CatalogDbContextMigrator(ICatalogDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public ICatalogDataSource DataSource { get => dataSource; set => dataSource = value; }

        public void Migrate()
        {
            if (ensureDeleted)
                DataSource.DbContext.Database.EnsureDeleted();
            if (!AllMigrationsApplied())
                DataSource.DbContext.Database.Migrate();
            else
                DataSource.DbContext.Database.EnsureCreated();
        }

        public async Task MigrateAsync()
        {
            if (ensureDeleted)
                await DataSource.DbContext.Database.EnsureDeletedAsync();
            if (!AllMigrationsApplied())
                await DataSource.DbContext.Database.MigrateAsync();
            else
                await DataSource.DbContext.Database.EnsureCreatedAsync();
        }

        public void DropDatabase()
        {
            DataSource.DbContext.Database.EnsureDeleted();
        }

        public async Task DropDatabaseAsync()
        {
            await DataSource.DbContext.Database.EnsureDeletedAsync();
        }

        public bool AllMigrationsApplied()
        {
            var applied = DataSource.DbContext.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = DataSource.DbContext.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public IEnumerable<string> GetAllMigrations()
        {
            var migrations = DataSource.DbContext.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(e => e.Key);
            return migrations.ToList();
        }

        public IEnumerable<string> GetAppliedMigrations()
        {
            var applied = DataSource.DbContext.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);
            return applied.ToList();
        }

        public IEnumerable<object> GetMigrations()
        {
            var applied = DataSource.DbContext.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var all = DataSource.DbContext.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            var migrations = applied.FullOuterJoin(all, a => a, b => b, (a, b, id) => new { Id = a == null ? b : a, Applied = a == null ? false : true });
            return migrations.ToList();
        }
    }
}