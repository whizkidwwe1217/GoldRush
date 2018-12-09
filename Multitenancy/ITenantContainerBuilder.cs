using System.Threading.Tasks;
using StructureMap;

namespace GoldRush.Multitenancy
{
    public interface ITenantContainerBuilder<TTenant>
    {
        Task<IContainer> BuildAsync(TTenant tenant);
    }
}