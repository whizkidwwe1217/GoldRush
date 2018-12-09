using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Security.Identity.Stores;
using GoldRush.Infrastructure.Security.Identity.Validators;

namespace GoldRush.Infrastructure.Security.Identity.Managers
{
    public class IdentityRoleManager : RoleManager<Role>
    {
        public IdentityRoleManager(IdentityRoleStore store,
            IEnumerable<IdentityRoleValidator> roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<RoleManager<Role>> logger,
            IHttpContextAccessor contextAccessor) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
            
        }

        public IdentityResult Create(Role role)
        {
            ThrowIfDisposed();
            return ((IdentityRoleStore) Store).Create(role);
        }

        public async Task<Role> FindByNameAsync(string roleName, Guid? companyId, Guid? tenantId)
        {
            ThrowIfDisposed();
            return await ((IdentityRoleStore) Store).FindByNameAsync(NormalizeKey(roleName), companyId, tenantId, CancellationToken);
        }

        public Role FindByName(string roleName, Guid? companyId, Guid? tenantId)
        {
            ThrowIfDisposed();
            return ((IdentityRoleStore) Store).FindByName(NormalizeKey(roleName), companyId, tenantId);
        }
    }
}