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
            await Groups.AddToGroupAsync(Context.ConnectionId, "General");

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userId = Guid.Parse(Context.UserIdentifier!);

            var isAdmin = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .AnyAsync(n => n == "Admin");

            if (isAdmin)
            {
                if (!await db.NotificationGroupMembers
                    .AnyAsync(m => m.UserId == userId && m.GroupName == "Admins"))
                {
                    db.NotificationGroupMembers.Add(
                        new NotificationGroupMember { UserId = userId, GroupName = "Admins" });
                    await db.SaveChangesAsync();
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "General");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
