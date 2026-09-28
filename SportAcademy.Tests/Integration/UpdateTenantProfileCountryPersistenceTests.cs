using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportAcademy.Application.Commands.TenantCommands.UpdateTenantProfile;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Integration;

// The handler's own unit tests mock ITenantRepository/IUnitOfWork, so they cannot tell whether
// the chosen Country actually reaches the database. This runs the real handler over the real
// repository and DbContext and reads the row back through a fresh context.
public class UpdateTenantProfileCountryPersistenceTests
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

    private static ApplicationDbContext CreateContext(Guid tenantId, string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var provider = new TestTenantIdProvider();
        provider.SetTenantId(tenantId);
        return new ApplicationDbContext(options, provider, new TestBranchAccessProvider());
    }

    [Fact]
    public async Task OnboardingSubmit_PersistsTheChosenCountry()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        using (var seed = CreateContext(tenantId, dbName))
        {
            seed.Set<Tenant>().Add(new Tenant
            {
                Id = tenantId, Name = "Acme", DisplayName = "Acme", Email = "a@example.com",
                Code = "ACME", Slug = "acme",
            });
            seed.Set<TenantProfile>().Add(new TenantProfile
                { TenantId = tenantId, OrganizationName = "Acme", IsSetupComplete = false });
            seed.Set<TenantSettings>().Add(new TenantSettings
            {
                TenantId = tenantId, TimeZone = "Asia/Kuwait", Language = "ar-KW",
                DateFormat = "dd/MM/yyyy", TimeFormat = "HH:mm", Currency = "KWD", Country = "KW",
            });
            await seed.SaveChangesAsync();
        }

        using (var ctx = CreateContext(tenantId, dbName))
        {
            var userContext = new Mock<IUserContextService>();
            userContext.Setup(u => u.TenantId).Returns(tenantId);
            var handler = new UpdateTenantProfileCommandHandler(
                new TenantRepository(ctx), new UnitOfWork(ctx), userContext.Object,
                Mock.Of<IFileStorageService>());

            var result = await handler.Handle(
                new UpdateTenantProfileCommand(
                    "Acme", "", null, "+201000000000", "", "Cairo", null, null, "",
                    MarkSetupComplete: true, Country: "EG"),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue(result.Message);
        }

        using var verify = CreateContext(tenantId, dbName);
        (await verify.Set<TenantSettings>().SingleAsync(s => s.TenantId == tenantId))
            .Country.Should().Be("EG");
        (await verify.Set<TenantProfile>().SingleAsync(p => p.TenantId == tenantId))
            .IsSetupComplete.Should().BeTrue();
    }
}
