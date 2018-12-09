using GoldRush.Core.Models;
using GoldRush.Core.Repositories;

namespace GoldRush.Infrastructure.Repositories
{
    public class SequenceNumberRepository : BaseCompanyRepository<int, SequenceNumber>
    {
        public SequenceNumberRepository(IRepositoryManager<int> repositoryManager) : base(repositoryManager)
        {
        }
    }
}