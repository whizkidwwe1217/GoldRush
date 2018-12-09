using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class UserLogin : IntBaseEntity
    {        
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}