using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core
{
    public interface ICatalogDataSource
    {
        DbContext DbContext { get; set; }    
    }
}