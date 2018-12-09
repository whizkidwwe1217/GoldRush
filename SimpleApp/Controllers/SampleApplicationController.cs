using System.Linq;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Multitenancy;
using GoldRush.Multitenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StructureMap;

namespace SimpleApp
{
    [Route("api/sample")]
    public class SampleApplicationController : ControllerBase
    {
        private readonly TenantContext<Tenant> tenantContext;
        private readonly IContainer container;

        public SampleApplicationController(TenantContext<Tenant> tenantContext, IContainer container)
        {
            this.tenantContext = tenantContext;
            this.container = container;
        }

        [Route("info")]
        public async Task<IActionResult> GetInfo()
        {
            var deps = container.WhatDoIHave();

            return Ok(await Task.FromResult(new
            {
                tenantContext,
                dependencies = deps
            }));
        }
    }
}