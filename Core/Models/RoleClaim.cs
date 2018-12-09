using System.Security.Claims;
using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class RoleClaim : IntBaseEntity
    {          
        public int RoleId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public Role Role { get; set; }
        public virtual Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }

        public virtual void InitializeFromClaim(Claim other)
        {
            ClaimType = other?.Type;
            ClaimValue = other?.Value;
        }
    }
}