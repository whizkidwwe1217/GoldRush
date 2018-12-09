using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure.Multitenancy
{
    public interface IActiveDbContext
    {
        DbContext DbContext { get; }
    }
}