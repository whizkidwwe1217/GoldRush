using GoldRush.Core.Models;
using GoldRush.Core.Controllers;
using System;
using GoldRush.Core.Services;

namespace GoldRush.Infrastructure.Controllers
{
    public class CompanyController : BaseServiceController<Guid, Company>
    {
        public CompanyController(IService<Guid, Company> service) : base(service)
        {
        }
    }
}