using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GoldRush.Core.Extensions;
using GoldRush.Core.Models;
using GoldRush.Multitenancy;
using Microsoft.AspNetCore.Mvc;

namespace GoldRush.Infrastructure.Controllers
{
    [Route("api/app/[controller]")]
    public class AssemblyController : ControllerBase
    {
        private readonly TenantContext<Tenant> tenantWrapper;
        private Tenant Tenant;

        public AssemblyController(TenantContext<Tenant> tenantWrapper)
        {
            this.tenantWrapper = tenantWrapper;
            this.Tenant = tenantWrapper.Tenant;
        }

        public class LoadedAssembly
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Namespace { get; set; }
            public string Location { get; set; }
            public FileVersionInfo FileVersionInfo { get; set; }
        }

        [HttpGet]
        public IEnumerable<LoadedAssembly> Get()
        {
            var assemblies = CoreModuleExtensions.GetLoadedAssemblies("GoldRush.Infrastructure");            
            return assemblies.Select(e => new LoadedAssembly
            {
                Name = e.GetName().Name,
                Location = e.Location,
                Version = FileVersionInfo.GetVersionInfo(e.Location).FileVersion,
                FileVersionInfo = FileVersionInfo.GetVersionInfo(e.Location)
            }).ToArray();
        }

        [HttpGet]
        [Route("[action]")]
        public IEnumerable<LoadedAssembly> GetEntityConfigurations()
        {
            var assemblies = CoreModuleExtensions.GetModuleEntityConfigurations("GoldRush.Infrastructure");            
            return assemblies.Select(e => new LoadedAssembly
            {
                Name = e.FullName,
                Namespace = e.Namespace,
                Location = e.Assembly.Location,
                Version = FileVersionInfo.GetVersionInfo(e.Assembly.Location).FileVersion,
                FileVersionInfo = FileVersionInfo.GetVersionInfo(e.Assembly.Location)
            }).ToArray();
        }
    }
}