using System;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Core.Services;
using GoldRush.Infrastructure.Repositories;

namespace GoldRush.Infrastructure.Services
{
    public interface ITenantService : IService<Guid, Tenant>
    {
        Task<SearchResponseData<Company>> GetCompanies(SearchParams parameters, 
            CancellationToken cancellationToken = default(CancellationToken));    
        Task<Company> GetCompany(Guid id,
            CancellationToken cancellationToken = default(CancellationToken));  
        Task Initialize(Tenant tenant, CancellationToken cancellationToken = default(CancellationToken));
    }

    public class TenantService : BaseService<Guid, Tenant>, ITenantService
    {
        public TenantService(ITenantRepository repository) : base(repository)
        {
        }

        public async Task<SearchResponseData<Company>> GetCompanies(SearchParams parameters, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ((ITenantRepository) Repository).GetCompanies(parameters, cancellationToken);
        }

        public async Task<Company> GetCompany(Guid id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ((ITenantRepository) Repository).GetCompany(id, cancellationToken);
        }

        public async Task Initialize(Tenant tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            await ((ITenantRepository) Repository).Initialize(tenant, cancellationToken);
        }
    }
}