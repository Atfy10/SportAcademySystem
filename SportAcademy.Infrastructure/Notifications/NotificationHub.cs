using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Notifications
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationHub(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public override async Task OnConnectedAsync()
        {
            var tenantId = GetTenantId();
            if (tenantId is null)
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.ForTenant(tenantId.Value, NotificationGroupNames.General));

            var userIdClaim = Context.UserIdentifier;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            {
                await base.OnConnectedAsync();
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var isAdmin = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .AnyAsync(n => n == "Admin");

            if (isAdmin)
            {
                var adminsGroup = NotificationGroupNames.ForTenant(tenantId.Value, NotificationGroupNames.Admins);

                if (!await db.NotificationGroupMembers
                    .AnyAsync(m => m.UserId == userId && m.GroupName == adminsGroup))
                {
                    db.NotificationGroupMembers.Add(
                        new NotificationGroupMember { UserId = userId, GroupName = adminsGroup });
                    await db.SaveChangesAsync();
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, adminsGroup);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var tenantId = GetTenantId();
            if (tenantId is not null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationGroupNames.ForTenant(tenantId.Value, NotificationGroupNames.General));
                // No explicit removal from the tenant's Admins group here: SignalR removes a
                // closed connection from every group it belongs to automatically.
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// Reads tenant_id from the connection's validated JWT claims - the same trustworthy
        /// source used everywhere else in the app (see UserContextService) - never from
        /// anything the client could supply.
        private Guid? GetTenantId()
        {
            var claim = Context.User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
        }
    }
}
