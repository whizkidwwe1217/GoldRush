using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class UserRole : IntBaseEntity
    {          
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public Role Role { get ; set; }
        public User User { get; set; }
    }
}