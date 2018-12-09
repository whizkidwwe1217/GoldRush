using System.Collections.Generic;

namespace GoldRush.Core.Models
{
    public class Role : IdentityRoleBase
    {
        public string Description { get; set; }
        public bool? IsSystemAdministrator { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public List<RoleClaim> RoleClaims { get; set; }
    }
}