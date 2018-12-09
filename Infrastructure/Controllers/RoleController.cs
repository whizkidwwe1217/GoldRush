using GoldRush.Core.Models;
using GoldRush.Core.Controllers;
using GoldRush.Infrastructure.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading;
using GoldRush.Core;
using GoldRush.Infrastructure.Security.Identity.Managers;
using Microsoft.AspNetCore.Identity;

namespace GoldRush.Infrastructure.Controllers
{
    public class RoleController : BaseServiceController<int, Role, IRoleService>
    {
        private readonly IdentityRoleManager roleManager;
        private readonly IdentityUserManager userManager;
        private readonly Tenant tenant;

        public RoleController(IRoleService service, IdentityRoleManager roleManager, IdentityUserManager userManager, Tenant tenant) : base(service)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.tenant = tenant;
        }

        public override async Task<IActionResult> Delete(int id)
        {
            Role role = await roleManager.FindByIdAsync(id.ToString());
            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return NoContent();
                }
            }
            ModelState.AddModelError("Errors", "Role doesn't exists.");
            return NotFound();
        }

        public override async Task<IActionResult> Create([FromBody] Role role)
        {
            if (ModelState.IsValid)
            {
                var found = await Service.AnyAsync(e => e.CompanyId == role.CompanyId && e.Name == role.Name);
                if (found)
                {
                    ModelState.AddModelError("Errors", "Role already exists.");
                    return StatusCode(StatusCodes.Status409Conflict, found);
                }
                role.TenantId = tenant.Id;
                IdentityResult result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return CreatedAtAction("Get", new { id = role.Id }, role);
                }
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("members")]
        public async Task<IActionResult> GetMembers([FromQuery] int roleId, [FromQuery] SearchParams parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await Service.GetMembersAsync(roleId, parameters, cancellationToken);
            if (result != null)
                return Ok(result);
            return NoContent();
        }

        [HttpPost]
        [Route("addmember")]
        public async Task<IActionResult> AddMember([FromBody] UserRole userRole, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ModelState.IsValid)
            {
                var found = await Service.FindRoleAsync(userRole);
                if (found != null)
                {
                    ModelState.AddModelError("Errors", "Member already exists.");
                    return StatusCode(StatusCodes.Status409Conflict, "Member already exists.");
                }
                await Service.AddMemberAsync(userRole, cancellationToken);
                return Ok(userRole);
            }
            ModelState.AddModelError("Errors", "Error adding member.");
            return BadRequest(ModelState);
        }
    }
}