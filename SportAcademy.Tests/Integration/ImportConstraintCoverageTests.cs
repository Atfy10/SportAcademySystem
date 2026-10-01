using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Services;

namespace SportAcademy.Tests.Integration;

// The CSV imports (trainees, employees) promise the database never rejects a row they passed: every column
// limit and unique index on the tables they write is checked up front by the import validators.
// These tests pin the real SQL Server model so that adding a new unique index or length limit
// fails here - pointing whoever added it at the import pre-checks - instead of surfacing to a
// user as "Couldn't be saved".
public class ImportConstraintCoverageTests
{
    private sealed class TenantProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; } = Guid.NewGuid();
        public bool AllowCrossTenantWrite => false;
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
        public IDisposable Impersonate(Guid tenantId) => new Noop();
        public IDisposable AllowCrossTenantOperation() => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }

    private sealed class BranchAccess : IBranchAccessProvider
    {
        public bool IsRestricted => false;
        public IReadOnlyList<int> AllowedBranchIds => [];
        public void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds) { }
    }

    private static ApplicationDbContext SqlServerModel()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=.;Database=NeverConnected;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options,
            new TenantProvider(), new BranchAccess());

    [Fact]
    public void UniqueIndexesOnTrainee_AreOnlyTheSystemGeneratedOnes()
    {
        using var ctx = SqlServerModel();
        var unique = ctx.Model.FindEntityType(typeof(Trainee))!
            .GetIndexes()
            .Where(i => i.IsUnique)
            .Select(i => string.Join(",", i.Properties.Select(p => p.Name)))
            .OrderBy(n => n)
            .ToList();

        // TraineeCode and AppUserId are generated/never set by an import. A new unique column
        // written from the CSV needs a matching duplicate check in TraineeImportValidator
        // (against the database AND within the file) before it's added to this list.
        unique.Should().BeEquivalentTo(["AppUserId", "TraineeCode"]);
    }

    [Fact]
    public void FieldLimitsUsedByTheImport_ComeFromTheRealModel()
    {
        using var ctx = SqlServerModel();
        var limits = new TraineeImportLookup(ctx).GetFieldLimits();

        limits.FirstName.Should().Be(50);
        limits.LastName.Should().Be(50);
        limits.Ssn.Should().Be(20);
        limits.PhoneNumber.Should().Be(20);
        limits.ParentNumber.Should().Be(20);
        limits.GuardianName.Should().Be(50);
        limits.Email.Should().Be(200);
        limits.Street.Should().Be(70);
        limits.City.Should().Be(50);
        limits.MedicalCondition.Should().Be(200);
    }

    [Fact]
    public void UniqueIndexesOnEmployee_AreOnlyTheSystemGeneratedOnes()
    {
        using var ctx = SqlServerModel();
        var unique = ctx.Model.FindEntityType(typeof(Employee))!
            .GetIndexes()
            .Where(i => i.IsUnique)
            .Select(i => string.Join(",", i.Properties.Select(p => p.Name)))
            .ToList();

        // AppUserId is never set by an import (no login accounts are created). A new unique column
        // written from the employee CSV needs a matching duplicate check in EmployeeImportValidator
        // (against the database AND within the file) before it's added to this list.
        unique.Should().BeEquivalentTo(["AppUserId"]);
    }

    [Fact]
    public void EmployeeFieldLimitsUsedByTheImport_ComeFromTheRealModel()
    {
        using var ctx = SqlServerModel();
        var limits = new EmployeeImportLookup(ctx).GetFieldLimits();

        limits.FirstName.Should().Be(50);
        limits.LastName.Should().Be(50);
        limits.Ssn.Should().Be(20);
        limits.PhoneNumber.Should().Be(20);
        limits.SecondPhoneNumber.Should().Be(20);
        limits.Email.Should().Be(200);
        limits.Street.Should().Be(70);
        limits.City.Should().Be(50);
    }
}
