using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core
{
    public class CatalogDataSource : ICatalogDataSource
    {
        private DbContext context;

        public CatalogDataSource(DbContext context)
        {
            this.context = context;
        }

        public DbContext DbContext { get => context; set => context = value; }
    }
}