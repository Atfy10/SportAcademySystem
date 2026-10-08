using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Infrastructure.Repositories;

// The trainee form's family picker searched by numeric family code only, and the handler threw
// on anything else - useless at a front desk where families are known by the guardian's name or
// phone. These pin the free-text search: code, family name, guardian name and guardian phone all
// find the family, and a term matching nothing is an empty list, not an error.
public class FamilyRepositorySearchTests
{
    private sealed class FixedTenant(Guid tenantId) : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; } = tenantId;
        public bool AllowCrossTenantWrite { get; private set; }
        public void SetTenantId(Guid? id) => TenantId = id;
        public IDisposable Impersonate(Guid id) => new Noop();
        public IDisposable AllowCrossTenantOperation()
        {
            AllowCrossTenantWrite = true;
            return new Noop();
        }

        private sealed class Noop : IDisposable
        {
            public void Dispose() { }
        }
    }

    private static (ApplicationDbContext Context, FamilyRepository Repository) Create()
    {
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options, new FixedTenant(tenantId), new Mock<IBranchAccessProvider>().Object);

        context.Families.AddRange(
            new Family { Id = 1, FamilyCode = 1001, Name = "Al-Saleh", GuardianName = "Khalid Al-Saleh", GuardianPhone = "96550001111", TenantId = tenantId },
            new Family { Id = 2, FamilyCode = 1002, Name = "Haddad", GuardianName = "Mona Haddad", GuardianPhone = "96560002222", TenantId = tenantId });
        context.SaveChanges();

        var language = new Mock<ICurrentLanguageProvider>();
        language.Setup(l => l.Language).Returns("en");
        return (context, new FamilyRepository(context, new Mock<IMapper>().Object, language.Object));
    }

    [Theory]
    [InlineData("1002")]          // family code
    [InlineData("Haddad")]        // family name
    [InlineData("Mona")]          // guardian name
    [InlineData("6000 2222")]     // guardian phone, typed with a space
    public async Task Search_FindsFamilyByCodeNameGuardianOrPhone(string term)
    {
        var (_, repository) = Create();

        var result = await repository.SearchFamiliesTranslatedAsync(term, 20);

        result.Should().ContainSingle().Which.Code.Should().Be(1002);
    }

    [Fact]
    public async Task Search_NoMatch_ReturnsEmptyList()
    {
        var (_, repository) = Create();

        var result = await repository.SearchFamiliesTranslatedAsync("Nobody", 20);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_ShortDigitRun_DoesNotMatchEveryPhone()
    {
        // "11" appears in one phone number, but two digits are too few to search phones by.
        var (_, repository) = Create();

        var result = await repository.SearchFamiliesTranslatedAsync("11", 20);

        result.Should().BeEmpty();
    }
}
