using Microsoft.EntityFrameworkCore;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Notifications;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Infrastructure.Repositories;

// Regression coverage for the S1 fix. NotificationRepository itself still has no tenant
// filter - it trusts the caller to pass an already tenant-scoped group name, which is exactly
// what NotificationHub and NotificationService now always do via NotificationGroupNames. These
// tests prove that contract end-to-end: a raw, unscoped group name still spans every tenant
// (documenting why nothing may ever pass one to this repository), while the scoped names the
// production code actually uses keep tenants fully apart.
public class NotificationRepositoryTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
    }

    private static ApplicationDbContext CreateContext(Guid? tenantId, string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var provider = new TestTenantIdProvider();
        if (tenantId.HasValue) provider.SetTenantId(tenantId.Value);

        return new ApplicationDbContext(options, provider);
    }

    private static AppUser CreateAdminUser(Guid tenantId, string userName) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        UserName = userName,
        Email = $"{userName}@test.com",
    };

    [Fact]
    public async Task AddRecipientsForGroupAsync_UnscopedGroupName_StillSpansMultipleTenants()
    {
        // Documents that the repository layer performs no tenant filtering on its own - this
        // is exactly why NotificationHub/NotificationService must never pass a raw, unscoped
        // group name (see the scoped-name test below for the production-realistic path).
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var adminA = CreateAdminUser(tenantA, "admin-tenant-a");
        var adminB = CreateAdminUser(tenantB, "admin-tenant-b");

        int notificationId;
        await using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Users.Add(adminA);
            seedCtx.Users.Add(adminB);
            await seedCtx.SaveChangesAsync();

            seedCtx.Set<NotificationGroupMember>().Add(new NotificationGroupMember { UserId = adminA.Id, GroupName = "Admins" });
            seedCtx.Set<NotificationGroupMember>().Add(new NotificationGroupMember { UserId = adminB.Id, GroupName = "Admins" });
            await seedCtx.SaveChangesAsync();

            var notification = new Notification
            {
                Message = "New subscription created",
                Title = "New Subscription",
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantA,
            };
            seedCtx.Set<Notification>().Add(notification);
            await seedCtx.SaveChangesAsync();
            notificationId = notification.Id;
        }

        await using (var repoCtx = CreateContext(tenantA, dbName))
        {
            var repository = new NotificationRepository(repoCtx, Mock.Of<AutoMapper.IMapper>());
            await repository.AddRecipientsForGroupAsync(notificationId, "Admins");
        }

        await using var verifyCtx = CreateContext(null, dbName);
        var recipientUserIds = await verifyCtx.Set<NotificationRecipient>()
            .Where(r => r.NotificationId == notificationId)
            .Select(r => r.UserId)
            .ToListAsync();

        Assert.Contains(adminA.Id, recipientUserIds);
        Assert.Contains(adminB.Id, recipientUserIds);
    }

    [Fact]
    public async Task AddRecipientsForGroupAsync_TenantScopedGroupName_NeverCrossesTenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var adminA = CreateAdminUser(tenantA, "admin-tenant-a");
        var adminB = CreateAdminUser(tenantB, "admin-tenant-b");

        var adminsGroupA = NotificationGroupNames.ForTenant(tenantA, NotificationGroupNames.Admins);
        var adminsGroupB = NotificationGroupNames.ForTenant(tenantB, NotificationGroupNames.Admins);
        Assert.NotEqual(adminsGroupA, adminsGroupB);

        int notificationId;
        await using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Users.Add(adminA);
            seedCtx.Users.Add(adminB);
            await seedCtx.SaveChangesAsync();

            // Mirrors NotificationHub.OnConnectedAsync: each admin lands in their own tenant's
            // scoped group, never a shared global one.
            seedCtx.Set<NotificationGroupMember>().Add(new NotificationGroupMember { UserId = adminA.Id, GroupName = adminsGroupA });
            seedCtx.Set<NotificationGroupMember>().Add(new NotificationGroupMember { UserId = adminB.Id, GroupName = adminsGroupB });
            await seedCtx.SaveChangesAsync();

            var notification = new Notification
            {
                Message = "New subscription created",
                Title = "New Subscription",
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantA,
            };
            seedCtx.Set<Notification>().Add(notification);
            await seedCtx.SaveChangesAsync();
            notificationId = notification.Id;
        }

        await using (var repoCtx = CreateContext(tenantA, dbName))
        {
            var repository = new NotificationRepository(repoCtx, Mock.Of<AutoMapper.IMapper>());
            // Mirrors NotificationService.SendNotificationToGroupAsync, which always scopes
            // "Admins" to the calling tenant before reaching the repository.
            await repository.AddRecipientsForGroupAsync(notificationId, adminsGroupA);
        }

        await using var verifyCtx = CreateContext(null, dbName);
        var recipientUserIds = await verifyCtx.Set<NotificationRecipient>()
            .Where(r => r.NotificationId == notificationId)
            .Select(r => r.UserId)
            .ToListAsync();

        Assert.Contains(adminA.Id, recipientUserIds);
        Assert.DoesNotContain(adminB.Id, recipientUserIds);
    }
}
