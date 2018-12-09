using GoldRush.Core.Licensing;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Core.Middlewares
{
    public class LicenseMiddleware
    {
        private readonly RequestDelegate next;

        public LicenseMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (LicenseManager.IsValid() || IsWhitelisted(context.Request.Path.Value)) 
                await next(context);
            else
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                LicenseManager.ThrowInvalidLicense();
            }
        }

        private bool IsWhitelisted(string value)
        {
            return value == "/" || value.StartsWith("/api/admin") 
                || (!value.StartsWith("/api/admin") && !value.StartsWith("/api"))
                || value.StartsWith("/api/app/assembly");
        }
    }
}
