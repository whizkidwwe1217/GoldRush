using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core
{
    public interface IDbContextMigrator
    {
        DbContext Context { get; set; }
        void Migrate();
    }
}