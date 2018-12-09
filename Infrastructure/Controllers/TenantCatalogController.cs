using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core;
using GoldRush.Core.Models;
using GoldRush.Data.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GoldRush.Infrastructure.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Policy = "TenantAdministrator")]
    public class TenantCatalogController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ICatalogDataSource dataSource;
        private readonly ICatalogDbContextMigrator migrator;

        public TenantCatalogController(IConfiguration configuration,
            ICatalogDataSource dataSource,
            ICatalogDbContextMigrator migrator)
        {
            this.configuration = configuration;
            this.dataSource = dataSource;
            this.migrator = migrator;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetConfiguration()
        {
            var options = configuration.GetSection("MultitenancyOptions");
            var catalogEngine = options.GetValue<DatabaseEngine>("CatalogEngine");
            var deploymentMode = options.GetValue<DeploymentModes>("DeploymentMode");
            var migrationAssembly = options.GetValue<string>("MigrationAssembly", "Migrations");
            var connectionString = configuration.GetConnectionString(options.GetValue<string>("ConnectionString"));

            var result = new
            {
                engine = catalogEngine.ToString(),
                multitenancyType = deploymentMode.ToString(),
                connectionString = connectionString,
                migrationAssembly = migrationAssembly
            };
            return Ok(await Task.FromResult(result));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Subscribe([FromBody] Tenant tenant)
        {
            await dataSource.DbContext.Set<Tenant>().AddAsync(tenant);
            await dataSource.DbContext.SaveChangesAsync();
            return Ok(tenant);
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await dataSource.DbContext.Set<Tenant>().AsNoTracking().ToListAsync(cancellationToken);
            return Ok(data);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] Tenant tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
                return BadRequest();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var options = configuration.GetSection("MultitenancyOptions");
                    var deploymentMode = options.GetValue<DeploymentModes>("DeploymentMode");
                    var connectionString = configuration.GetConnectionString(options.GetValue<string>("ConnectionString"));

                    if (deploymentMode == DeploymentModes.Multi)
                        tenant.IsIsolated = true;
                    else if (deploymentMode == DeploymentModes.Single)
                    {
                        tenant.ConnectionString = connectionString;
                        tenant.IsIsolated = false;
                    }

                    await dataSource.DbContext.Set<Tenant>().AddAsync(tenant);
                    await dataSource.DbContext.SaveChangesAsync(cancellationToken);
                    return CreatedAtAction(nameof(Get), tenant);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new Exception("This record has been modified by another user. Error details: " + ex.Message);
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.InnerException.Message, details = ex.InnerException });
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Tenant tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null || !tenant.Id.Equals(id))
            {
                return BadRequest();
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var e = await dataSource.DbContext.FindAsync<Tenant>(new object[] { id }, cancellationToken);
                var options = configuration.GetSection("MultitenancyOptions");
                var deploymentMode = options.GetValue<DeploymentModes>("DeploymentMode");
                var connectionString = configuration.GetConnectionString(options.GetValue<string>("ConnectionString"));

                if (deploymentMode == DeploymentModes.Multi)
                    tenant.IsIsolated = true;
                else if (deploymentMode == DeploymentModes.Single)
                    tenant.ConnectionString = connectionString;

                tenant.DateModified = DateTime.UtcNow;
                dataSource.DbContext.Entry<Tenant>(e).CurrentValues.SetValues(tenant);
                await dataSource.DbContext.SaveChangesAsync(cancellationToken);

                /*
                    Update tenant in isolation mode
                */
                if (tenant.IsIsolated)
                {
                    DbContext dbContext = dataSource.DbContext;

                    switch (tenant.Engine)
                    {
                        case DatabaseEngine.SqlServer:
                            dbContext = new SqlServerDbContext(configuration, tenant, new DbContextOptions<SqlServerDbContext>());
                            break;
                        case DatabaseEngine.MySql:
                            dbContext = new MySqlDbContext(configuration, tenant, new DbContextOptions<MySqlDbContext>());
                            break;
                        case DatabaseEngine.PostgreSql:
                            dbContext = new PostgreSqlDbContext(configuration, tenant, new DbContextOptions<PostgreSqlDbContext>());
                            break;
                    }

                    try
                    {
                        var hostedTenant = await dbContext.Set<Tenant>()
                            .Where(ht => ht.HostName == tenant.HostName)
                            .FirstOrDefaultAsync();
                        if (hostedTenant != null)
                        {
                            dbContext.Entry<Tenant>(hostedTenant).State = EntityState.Modified;
                            hostedTenant.Active = tenant.Active;
                            hostedTenant.ConnectionString = tenant.ConnectionString;
                            hostedTenant.DateCreated = tenant.DateCreated;
                            hostedTenant.DateModified = tenant.DateModified;
                            hostedTenant.DateDeleted = tenant.DateDeleted;
                            hostedTenant.Deleted = tenant.Deleted;
                            hostedTenant.DeploymentStatus = tenant.DeploymentStatus;
                            hostedTenant.Description = tenant.Description;
                            hostedTenant.Name = tenant.Name;
                            hostedTenant.HostName = tenant.HostName;
                            hostedTenant.Edition = tenant.Edition;
                            hostedTenant.Engine = tenant.Engine;
                            hostedTenant.IsIsolated = tenant.IsIsolated;
                            hostedTenant.IsTenantAdministrator = tenant.IsTenantAdministrator;
                            hostedTenant.SubscriptionId = tenant.SubscriptionId;
                            hostedTenant.Theme = tenant.Theme;
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new AcceptedResult(nameof(Update), ex.Message + ". " + ex.InnerException?.Message);
                    }
                }

                HttpContext.Response.Headers.Add("ConcurrencyStamp", Convert.ToBase64String(tenant.ConcurrencyStamp));
                HttpContext.Response.Headers.Add("ConcurrencyTimeStamp", tenant.ConcurrencyTimeStamp.ToString());
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.InnerException.Message, details = ex.InnerException });
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.InnerException.Message, details = ex.InnerException });
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }

            return new NoContentResult();
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenant = await dataSource.DbContext.FindAsync<Tenant>(new object[] { id }, cancellationToken);
            if (tenant == null)
                return NotFound(tenant);
            return Ok(tenant);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var persistedTenant = await dataSource.DbContext.FindAsync<Tenant>(new object[] { id }, cancellationToken);
            if (persistedTenant == null)
            {
                return NotFound();
            }

            dataSource.DbContext.Remove(persistedTenant);
            await dataSource.DbContext.SaveChangesAsync(cancellationToken);
            return new NoContentResult();
        }

        [HttpGet]
        [Route("[action]")]
        public virtual async Task<ActionResult<SearchResponseData>> Search([FromQuery] SearchParams parameter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dataSource.DbContext.Set<Tenant>()
                .Search<Tenant, Guid>(parameter)
                .ToListAsync<Tenant>(parameter, cancellationToken);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                await dataSource.DbContext.Database.ExecuteSqlCommandAsync("SELECT 1");
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
                var sql = MigrationUtils.GenerateCreateScript(this.dataSource.DbContext.Database);
                return Ok(await Task.FromResult(sql));
            }
            catch (Exception ex)
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
                await dataSource.DbContext.Database.EnsureDeletedAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return this.FailStatus(StatusCodes.Status500InternalServerError,
                    "An error has occurred while dropping database.", ex);
            }
        }

        public class CatalogLogin
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] CatalogLogin login)
        {
            var claims = new Claim[] {
                new Claim(ClaimTypes.Email, login.Email),
                new Claim(JwtRegisteredClaimNames.Iss, configuration["AdminTokenAuthentication:Issuer"])
            };
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Email, login.Email));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });
            return Ok(await Task.FromResult(0));
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GenerateSecret()
        {
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[256 / 8];
            rng.GetBytes(bytes);
            return Ok(await Task.FromResult(Convert.ToBase64String(bytes)));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateToken([FromBody] CatalogLogin model)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(JwtRegisteredClaimNames.Iss, configuration["AdminTokenAuthentication:Issuer"])
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AdminTokenAuthentication:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationBuffer = configuration["AdminTokenAuthentication:Expiration"] != null ? int.Parse(configuration["AdminTokenAuthentication:Expiration"]) : 3;
            var expiryDate = DateTime.UtcNow.AddMinutes(expirationBuffer);
            var token = new JwtSecurityToken(configuration["AdminTokenAuthentication:Issuer"],
              configuration["AdminTokenAuthentication:Audience"],
              claims,
              expires: expiryDate,
              signingCredentials: creds);

            if (model.Email == "admin@live.com" && model.Password == "12345678")
            {
                var result = await Task.FromResult(new
                {
                    access_token = new JwtSecurityTokenHandler().WriteToken(token),
                    expirationBuffer,
                    expiryDate
                });
                return Ok(result);
            }
            return Unauthorized();
        }
    }
}