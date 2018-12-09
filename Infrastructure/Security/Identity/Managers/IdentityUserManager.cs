using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Security.Identity.Stores;
using GoldRush.Infrastructure.Security.Identity.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoldRush.Infrastructure.Security.Identity.Managers
{
    public class IdentityUserManager : UserManager<User>
    {
        public IdentityUserManager(IdentityUserStore store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<User> passwordHasher, 
            IEnumerable<IdentityUserValidator> userValidators, 
            IEnumerable<IPasswordValidator<User>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            
        }

        public IdentityResult Create(User user)
        {
            ThrowIfDisposed();
            return ((IdentityUserStore) Store).Create(user);
        }

        public async Task<User> FindByIdAsync(int userId)
        {
            ThrowIfDisposed();
            return await Store.FindByIdAsync(userId.ToString(), CancellationToken);
        }

        public async Task<User> FindByNameAsync(string userName, Guid? companyId, Guid? tenantId)
        {
            ThrowIfDisposed();
            return await ((IdentityUserStore) Store).FindByNameAsync(NormalizeKey(userName), companyId, tenantId, CancellationToken);
        }

        public User FindByName(string userName, Guid? companyId, Guid? tenantId)
        {
            ThrowIfDisposed();
            return ((IdentityUserStore) Store).FindByName(NormalizeKey(userName), companyId, tenantId);
        }

        public async Task<User> FindByEmailAsync(string email, Guid? companyId, Guid? tenantId)
        {
            ThrowIfDisposed();
            return await ((IdentityUserStore) Store).FindByEmailAsync(NormalizeKey(email), companyId, tenantId);
        }
    }
}