using System;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Controllers;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Infrastructure.Controllers
{
    public class TenantController : BaseServiceController<Guid, Tenant, ITenantService>
    {
        private readonly ITenantDbContextMigrator migrator;

        public TenantController(ITenantService service, ITenantDbContextMigrator migrator) : base(service)
        {
            this.migrator = migrator;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("[action]")]
        public async Task<SearchResponseData<Company>> Companies([FromQuery] SearchParams parameters, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Service.GetCompanies(parameters, cancellationToken);
        }

        [AllowAnonymous]
        [HttpGet("companies/{id}")]
        public async Task<Company> Companies(Guid id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Service.GetCompany(id, cancellationToken);
        }

        [AllowAnonymous]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Initialize([FromBody] Tenant tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await Service.Initialize(tenant, cancellationToken);
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                await Service.Repository.DbContext.Database.ExecuteSqlCommandAsync("SELECT 1");
                return Ok("Ok");
            }
            catch (Exception ex)
            {
                return this.FailStatus(StatusCodes.Status503ServiceUnavailable,
                    "This is service is not available. Make sure that the database exists by running database migrations. See details for more info.",
                    ex);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GenerateScript()
        {
            try
            {
                var sql = MigrationUtils.GenerateCreateScript(this.Service.Repository.DbContext.Database);
                return Ok(await Task.FromResult(sql));
            }
            catch(Exception ex)
            {
                return this.FailStatus(StatusCodes.Status500InternalServerError, "An error has occurred while generating a migration script.", ex);
            }
        }
        

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Migrate()
        {
            try
            {
                await migrator.MigrateAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return this.FailStatus(StatusCodes.Status500InternalServerError,
                    "An error has occurred while applying database migrations.", ex);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetAllMigrations()
        {
            return Ok(await Task.FromResult(migrator.GetAllMigrations()));
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetAppliedMigrations()
        {
            return Ok(await Task.FromResult(migrator.GetAppliedMigrations()));
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMigrations()
        {
            return Ok(await Task.FromResult(migrator.GetMigrations()));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Drop()
        {
            try
            {
                await this.Service.Repository.DbContext.Database.EnsureDeletedAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return this.FailStatus(StatusCodes.Status500InternalServerError,
                    "An error has occurred while dropping database.", ex);
            }
        }
    }
}