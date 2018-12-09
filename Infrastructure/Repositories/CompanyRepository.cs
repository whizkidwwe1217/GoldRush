using System;
using GoldRush.Core.Models;
using GoldRush.Core.Repositories;

namespace GoldRush.Infrastructure.Repositories
{
    public interface ICompanyRepository : IRepository<Guid, Company> {}
    public class CompanyRepository : BaseRepository<Guid, Company>, ICompanyRepository
    {
        public CompanyRepository(IRepositoryManager<Guid> repositoryManager) : base(repositoryManager)
        {
        }
    }
}