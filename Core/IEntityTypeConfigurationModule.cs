using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core
{
    public interface IEntityTypeConfigurationModule : IModule
    {
        void ApplyConfigurations(ModelBuilder builder, bool useRowVersion);
    }
}