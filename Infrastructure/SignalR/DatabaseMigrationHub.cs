using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GoldRush.Infrastructure.SignalR
{
    [Authorize(Policy = "TenantAdministrator", AuthenticationSchemes = "Bearer")]
    public class DatabaseMigrationHub : Hub
    {
    }
}