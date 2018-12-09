using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GoldRush.Core.Security
{
    /**
        How to use this attribute
        [Route("api/resource")]
        public class MyController : Controller
        {
            [ClaimRequirement(MyClaimTypes.Permission, "CanReadResource")]
            [HttpGet]
            public IActionResult GetResource()
            {
                return Ok();
            }
        }
     */
    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute(string claimType, string claimValue) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new Claim(claimType, claimValue) };
        }
    }
}