using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoldRush.Core.Models
{
    public class User : IdentityUserBase
    {
        public User() {}
        public User(Guid companyId, string userName)
        {
            this.UserName = userName;
            this.IsSystemAdministrator = false;
            this.CompanyId = companyId;
        }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]        
        public string ConfirmPassword { get; set; }
        public string MobileNo { get; set; }
        public string RecoveryEmail { get; set; }
        public bool? IsConfirmed { get; set; }
        public bool? IsSystemAdministrator { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public List<UserClaim> UserClaims { get; set; }
        public List<UserLogin> UserLogins { get; set; }
        public List<UserToken> UserTokens { get; set; }
    }
}