using GoldRush.Core.Models;

namespace GoldRush.Core
{
    public class AppInfo
    {
        public Tenant Tenant { get; set; }
        public bool HasInitialized { get; set; }
    }   
}