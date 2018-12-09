using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using GoldRush.Core.Security;
using GoldRush.Infrastructure.Services;
using GoldRush.Infrastructure.Security.Identity.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using GoldRush.Core.Models;
using GoldRush.Infrastructure.ViewModels;
using System.Security.Claims;

namespace GoldRush.Infrastructure.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DbContext dbContext;
        private readonly IConfiguration configuration;
        private readonly SignInManager<User> signInManager;
        private readonly IdentityUserManager userManager;
        private readonly IEmailSender emailSender;
        private readonly ICompanyService companyService;
        private readonly Tenant tenant;

        public AccountController(DbContext dbContext, IConfiguration configuration,
            IdentityUserManager userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ICompanyService companyService,
            Tenant tenant)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.companyService = companyService;
            this.tenant = tenant;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (model == null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.UserName, model.CompanyId, tenant.Id);

                if (user == null)
                {
                    ModelState.AddModelError(model.UserName, "Account does not exists.");
                    return Unauthorized();
                }

                var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {
                    //return Ok(user);
                    return Accepted();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unauthorized login.");
                return BadRequest();
            }
            ModelState.AddModelError(string.Empty, "Unauthorized login.");
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.UserName, model.CompanyId, tenant.Id);

            if (user == null)
            {
                ModelState.AddModelError(model.UserName, "Account does not exists.");
                return Unauthorized();
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.RequiresTwoFactor)
            {
                return StatusCode(StatusCodes.Status501NotImplemented);
            }
            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked);
            }
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var userClaims = await userManager.GetClaimsAsync(user);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(CompanyClaimTypes.Company, user.CompanyId.ToString()),
                new Claim(CompanyClaimTypes.Tenant, user.TenantId.ToString())
            }.Union(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenAuthentication:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationBuffer = configuration["TokenAuthentication:Expiration"] != null ? int.Parse(configuration["TokenAuthentication:Expiration"]) : 3;
            var expiryDate = DateTime.UtcNow.AddMinutes(expirationBuffer);
            var token = new JwtSecurityToken(
                issuer: configuration["TokenAuthentication:Issuer"],
                audience: configuration["TokenAuthentication:Audience"],
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds);

            var refreshToken = userManager.PasswordHasher.HashPassword(user, Guid.NewGuid().ToString())
                .Replace("+", string.Empty)
                .Replace("=", string.Empty)
                .Replace("/", string.Empty);
            // https://github.com/spetz/tokenmanager-sample/blob/master/src/TokenManager.Api/Services/AccountService.cs
            return Ok(new
            {
                client_id = user.Id,
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                expirationBuffer,
                expiryDate,
                refresh_token = new { refreshToken, expirationBuffer = 86400, expiryDate = DateTime.UtcNow.AddMinutes(86400) }, // Refresh token will expire for a day
                claims = claims.Select(e => new { e.Type, e.Issuer, e.Value })
            });
        }

        public (int, string) RefreshAccessToken(string token)
        {
            return (1, "");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("[action]")]
        public async Task<IActionResult> Register([FromBody] AccountViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var user = new User()
                    {
                        CompanyId = model.CompanyId,
                        TenantId = tenant.Id,
                        Active = model.Active,
                        Email = model.Email,
                        Password = model.Password,
                        ConfirmPassword = model.ConfirmPassword,
                        RecoveryEmail = model.RecoveryEmail,
                        MobileNo = model.MobileNo,
                        IsSystemAdministrator = model.IsSystemAdministrator,
                        IsConfirmed = false,
                        UserName = model.UserName,
                        LockoutEnabled = model.LockoutEnabled,
                        TwoFactorEnabled = model.TwoFactorEnabled
                    };

                    var companies = await companyService.ListAsync();
                    if (companies.Count == 0)
                    {
                        var company = new Company
                        {
                            Code = tenant.Name,
                            Name = tenant.Name,
                            TenantId = tenant.Id
                        };
                        await companyService.AddAsync(company);
                        await companyService.SaveAsync();
                        user.CompanyId = company.Id;
                    }

                    var result = await userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var confirmationCode = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(
                            controller: "Account",
                            action: "ConfirmEmail",
                            values: new { userId = user.Id, code = confirmationCode },
                            protocol: Request.Scheme);
                        await emailSender.SendEmailAsync(
                            email: user.Email,
                            subject: "Confirm Email",
                            htmlMessage: callbackUrl
                        );

                        return Ok(new { message = "User account registered successfully.", success = true });
                    }
                    // await context.Users.AddAsync(user);
                    // await context.SaveChangesAsync();
                    return BadRequest(new
                    {
                        message = "Failed to register user account.",
                        errors = result.Errors,
                        success = false
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new
                        {
                            message = "An error has occurred while registering a new user.",
                            success = false,
                            details = ex.Message
                        });
                }
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return BadRequest();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return Ok();
            return BadRequest();
        }

        [HttpGet("{id}")]
        [Route("[action]")]
        public async Task<IActionResult> GetClaims([FromQuery] int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var claims = await userManager.GetClaimsAsync(user);
                return Ok(claims);
            }
            return BadRequest();
        }

        [HttpPost()]
        [Route("[action]")]
        public async Task<IActionResult> CreateClaim([FromBody] ClaimViewModel claimViewModel)
        {
            var user = await userManager.FindByIdAsync(claimViewModel.UserId.ToString());
            if (user != null)
            {
                var claim = new Claim(claimViewModel.Type, claimViewModel.Value);
                var result = await userManager.AddClaimAsync(user, claim);
                if (result.Succeeded)
                {
                    return Ok(claim);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, result.Errors.First().Description);
            }
            return BadRequest();
        }
    }
}