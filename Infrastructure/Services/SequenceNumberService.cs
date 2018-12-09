using GoldRush.Core.Models;
using GoldRush.Core.Repositories;
using GoldRush.Core.Services;

namespace GoldRush.Infrastructure.Services
{
    public class SequenceNumberService : BaseService<int, SequenceNumber>
    {
        public SequenceNumberService(IRepository<int, SequenceNumber> repository) : base(repository)
        {
        }
    }
}