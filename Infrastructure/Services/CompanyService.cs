using System;
using GoldRush.Core.Models;
using GoldRush.Core.Services;
using GoldRush.Infrastructure.Repositories;

namespace GoldRush.Infrastructure.Services
{
    public interface ICompanyService : IService<Guid, Company> { }
    public class CompanyService : BaseService<Guid, Company>, ICompanyService
    {
        public CompanyService(ICompanyRepository repository) : base(repository)
        {
        }
    }
}