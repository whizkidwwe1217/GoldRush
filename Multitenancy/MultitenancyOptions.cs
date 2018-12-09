using GoldRush.Core;

namespace GoldRush.Multitenancy
{
    public class MultitenancyOptions
    {
        public MultitenancyOptions()
        {
            CatalogEngine = DatabaseEngine.SqlServer;
            DeploymentMode = DeploymentModes.Single;
        }
        public DatabaseEngine CatalogEngine { get; set; }
        public DeploymentModes DeploymentMode { get; set; }    
    }
}