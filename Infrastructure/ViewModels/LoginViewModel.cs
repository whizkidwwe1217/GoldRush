using System;

namespace GoldRush.Infrastructure.ViewModels
{
    public class LoginViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
    }
}