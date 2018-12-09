using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Security.Identity.Managers;
using Microsoft.AspNetCore.Identity;

namespace GoldRush.Infrastructure.Security.Identity.Validators
{
    public class IdentityRoleValidator : IRoleValidator<Role>
    {
        public IdentityRoleValidator(IdentityErrorDescriber errors = null)
        {
            Describer = errors ?? new IdentityErrorDescriber();
        }
        public IdentityErrorDescriber Describer { get; private set; }
        public virtual async Task<IdentityResult> ValidateAsync(RoleManager<Role> manager, Role role)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            var errors = new List<IdentityError>();
            await ValidateRoleName(manager as IdentityRoleManager, role, errors);
            if (errors.Count > 0)
            {
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
        }

        private async Task ValidateRoleName(IdentityRoleManager manager, Role role,
            ICollection<IdentityError> errors)
        {
            var roleName = await manager.GetRoleNameAsync(role);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                errors.Add(Describer.InvalidRoleName(roleName));
            }
            else
            {
                var owner = await manager.FindByNameAsync(roleName, role.CompanyId, role.TenantId);
                if (owner != null && 
                    !string.Equals(await manager.GetRoleIdAsync(owner), await manager.GetRoleIdAsync(role)))
                {
                    errors.Add(Describer.DuplicateRoleName(roleName));
                }
            }
        }
    }
}
