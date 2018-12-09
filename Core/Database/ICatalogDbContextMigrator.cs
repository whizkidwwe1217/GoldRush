using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoldRush.Core
{
    public interface ICatalogDbContextMigrator
    {
        ICatalogDataSource DataSource { get; set; }
        void Migrate();
        Task MigrateAsync();
        bool AllMigrationsApplied();
        IEnumerable<string> GetAllMigrations();
        IEnumerable<string> GetAppliedMigrations();
        IEnumerable<object> GetMigrations();
    }
}