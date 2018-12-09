using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class UserToken : IntBaseEntity
    {
        public int UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public User User { get; set; }
    }
}