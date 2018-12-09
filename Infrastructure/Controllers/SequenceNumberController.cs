using GoldRush.Core.Controllers;
using GoldRush.Core.Models;
using GoldRush.Core.Services;

namespace GoldRush.Infrastructure.Controllers
{
    public class SequenceNumberController : BaseServiceController<int, SequenceNumber>
    {
        public SequenceNumberController(IService<int, SequenceNumber> service) : base(service)
        {
        }
    }
}