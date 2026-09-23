using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;
using SportAcademy.Application.Commands.UserCommands.UserDelete;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Integration;

// CreateTraineeCommandHandler inserts the trainee's AppUser row directly via
// IUserRepository.AddAsyncWithoutSave + SaveChanges, never through UserManager.CreateAsync -
// so NormalizedUserName/NormalizedEmail are never populated for it (Identity sets those itself
// inside CreateAsync/UpdateAsync, not on plain insert). This reproduces that exact row and then
// drives ToggleUserActiveCommandHandler - the same handler AdminController's "activate/
// deactivate" button calls - over a *real* UserManager<AppUser>, not a mock, to see whether
// Identity's own update path actually persists the change against a row shaped like that.
public class ToggleUserActiveOnTraineeCreatedAccountTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public bool AllowCrossTenantWrite { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
        public IDisposable Impersonate(Guid tenantId) => new Noop();
        public IDisposable AllowCrossTenantOperation() => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }

    private sealed class TestBranchAccessProvider : IBranchAccessProvider
    {
        public bool IsRestricted => false;
        public IReadOnlyList<int> AllowedBranchIds => [];
        public void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds) { }
    }

    private static ServiceProvider BuildProvider(string dbName, Guid tenantId, bool withSoftDeleteInterceptor = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantIdProvider>(_ =>
        {
            var p = new TestTenantIdProvider();
            p.SetTenantId(tenantId);
            return p;
        });
        services.AddSingleton<IBranchAccessProvider, TestBranchAccessProvider>();

        if (withSoftDeleteInterceptor)
        {
            // Same wiring as Program.cs: IUserContextService -> SoftDeleteInterceptor ->
            // registered on the context's options, so a plain `Context.Remove(...)` (which is
            // all UserManager.DeleteAsync does under the hood) goes through the same
            // Deleted-to-soft-deleted conversion it would in production.
            var userContext = new Mock<IUserContextService>();
            userContext.Setup(u => u.UserId).Returns((Guid?)null);
            services.AddSingleton(userContext.Object);
            services.AddScoped<SoftDeleteInterceptor>();
            services.AddDbContext<ApplicationDbContext>((sp, o) =>
                o.UseInMemoryDatabase(dbName)
                    .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        }

        // Same shape as Program.cs's AddIdentity<AppUser, AppRole>(...) call - in particular the
        // same AllowedUserNameCharacters override, since that's the one Identity option this
        // repo already had to touch for Arabic names.
        services.AddIdentity<AppUser, AppRole>(o => o.User.AllowedUserNameCharacters = string.Empty)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task ToggleActive_OnAccountInsertedLikeCreateTraineeDoes_ActuallyPersists()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var provider = BuildProvider(dbName, tenantId))
        {
            var ctx = provider.GetRequiredService<ApplicationDbContext>();
            ctx.Set<Tenant>().Add(new Tenant
            {
                Id = tenantId, Name = "Acme", DisplayName = "Acme", Email = "a@example.com",
                Code = "ACME", Slug = "acme",
            });

            // The exact shape CreateTraineeCommandHandler builds and inserts today: a bare
            // `new AppUser { ... }` added straight through the repository, never through
            // UserManager.CreateAsync. NormalizedUserName/NormalizedEmail are left at their
            // C# default (null), exactly as they are after that insert in production.
            var appUser = new AppUser
            {
                Id = userId,
                TenantId = tenantId,
                UserName = "amiraahmed",
                Email = "amira.guardian@example.com",
                PhoneNumber = "+201000000000",
                IsBanned = false,
            };
            var hasher = provider.GetRequiredService<IPasswordHasher<AppUser>>();
            appUser.PasswordHash = hasher.HashPassword(appUser, "Temp1234!");
            ctx.Set<AppUser>().Add(appUser);
            await ctx.SaveChangesAsync();
        }

        using (var provider = BuildProvider(dbName, tenantId))
        {
            var ctx = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<AppRole>>();
            var userRepository = new UserRepository(ctx, userManager, roleManager);

            var userContext = new Mock<IUserContextService>();
            userContext.Setup(u => u.TenantId).Returns(tenantId);
            userContext.Setup(u => u.UserId).Returns((Guid?)null);
            var limitService = new Mock<IEffectiveLimitService>();
            limitService
                .Setup(s => s.GetAsync(tenantId, LimitedResources.Users, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EffectiveLimit(LimitedResources.Users, null, LimitSource.Unlimited, 0, null));
            var publisher = new Mock<IPublisher>();

            var handler = new ToggleUserActiveCommandHandler(
                userRepository, userContext.Object, limitService.Object, publisher.Object);

            var result = await handler.Handle(new ToggleUserActiveCommand(userId), CancellationToken.None);

            result.IsSuccess.Should().BeTrue(result.Message);
            result.Data.Should().BeFalse("the handler reports the new isActive state, and the user was just banned");
        }

        using (var provider = BuildProvider(dbName, tenantId))
        {
            var ctx = provider.GetRequiredService<ApplicationDbContext>();
            var reread = await ctx.Set<AppUser>().SingleAsync(u => u.Id == userId);
            reread.IsBanned.Should().BeTrue("the toggle must actually reach the database, not just report success");
        }
    }

    // "Delete user" (UserController's [HttpDelete]) goes through the same UserRepository.DeleteAsync
    // -> UserManager.DeleteAsync -> Context.Remove(user) path - which, for a user who has ever
    // received a notification, would hit NotificationRecipient.UserId's OnDelete(Restrict) FK on
    // an actual row removal. AppUser also implements ISoftDeletable, and SoftDeleteInterceptor
    // (registered in Program.cs) converts that Deleted state to a soft-delete before it reaches
    // the database - this proves that conversion is what keeps the FK from ever being evaluated.
    [Fact]
    public async Task DeleteUser_WhoHasNotificationHistory_SoftDeletesInsteadOfHittingTheRestrictFK()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var provider = BuildProvider(dbName, tenantId, withSoftDeleteInterceptor: true))
        {
            var ctx = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<AppUser>>();

            ctx.Set<Tenant>().Add(new Tenant
            {
                Id = tenantId, Name = "Acme", DisplayName = "Acme", Email = "a@example.com",
                Code = "ACME", Slug = "acme",
            });
            await ctx.SaveChangesAsync();

            // A normally-created account this time (through UserManager, like a real staff
            // login), with a notification actually sent to them - an ordinary, unremarkable
            // history for any active user.
            var user = new AppUser { Id = userId, TenantId = tenantId, UserName = "coach.sara", Email = "sara@example.com" };
            (await userManager.CreateAsync(user, "Temp1234!")).Succeeded.Should().BeTrue();

            var notification = new Notification { TenantId = tenantId, Message = "Welcome" };
            notification.Recipients.Add(new NotificationRecipient { UserId = userId });
            ctx.Set<Notification>().Add(notification);
            await ctx.SaveChangesAsync();
        }

        using (var provider = BuildProvider(dbName, tenantId, withSoftDeleteInterceptor: true))
        {
            var ctx = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<AppRole>>();
            var userRepository = new UserRepository(ctx, userManager, roleManager);

            var handler = new DeleteUserCommandHandler(userRepository);

            var act = () => handler.Handle(new DeleteUserCommand(userId), CancellationToken.None);

            await act.Should().NotThrowAsync("a user with notification history must still be deletable");
        }

        using (var provider = BuildProvider(dbName, tenantId, withSoftDeleteInterceptor: true))
        {
            var ctx = provider.GetRequiredService<ApplicationDbContext>();

            // IgnoreQueryFilters: the row must still exist (soft-deleted), not be physically gone -
            // that's what avoids the Restrict FK in the first place.
            var reread = await ctx.Set<AppUser>().IgnoreQueryFilters().SingleAsync(u => u.Id == userId);
            reread.IsDeleted.Should().BeTrue();

            // And the normal, filtered query - what GetAllUsersQuery uses - no longer sees them.
            (await ctx.Set<AppUser>().AnyAsync(u => u.Id == userId)).Should().BeFalse();
        }
    }
}
