using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface ITenantDbContextMigrator
    {
        DbContext DbContext { get; set; }
        void Migrate();
        Task MigrateAsync();
        bool AllMigrationsApplied();
        IEnumerable<string> GetAllMigrations();
        IEnumerable<string> GetAppliedMigrations();
        IEnumerable<object> GetMigrations();
    }