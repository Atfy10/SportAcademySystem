using FluentAssertions;
using FluentValidation;
using Moq;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.DTOs.ImportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services.TraineeImport;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Tests.Application.Services;

public class TraineeImportValidatorTests
{
    // Returns the message KEY (so tests assert which rule fired, not wording) and real column
    // labels for the header index.
    private sealed class KeyLocalizer : ILocalizationService
    {
        private static readonly Dictionary<string, string> Labels = new()
        {
            ["en:import.column.FirstName"] = "First Name",
            ["ar:import.column.FirstName"] = "الاسم الأول",
            ["en:import.column.LastName"] = "Last Name",
            ["en:import.column.BirthDate"] = "Birth Date",
            ["en:import.column.PhoneNumber"] = "Phone",
            ["en:import.column.NationalityCategory"] = "Nationality Category",
            ["en:enum.Gender.Male"] = "Male",
            ["ar:enum.Gender.Male"] = "ذكر",
            ["en:enum.Gender.Female"] = "Female",
            ["en:enum.Nationality.Kuwaiti"] = "Kuwaiti",
            ["ar:enum.Nationality.Kuwaiti"] = "كويتي",
        };

        public string this[string key, params object[] args] => key;
        public string GetIn(string language, string key, params object[] args)
            => Labels.TryGetValue($"{language}:{key}", out var v) ? v : key;
        public bool Exists(string key) => false;
    }

    private readonly Mock<ITraineeImportLookup> _lookup = new();
    private readonly Mock<IEffectiveLimitService> _limits = new();
    private readonly Mock<IUserContextService> _user = new();
    private readonly Mock<IBranchAccessProvider> _branchAccess = new();
    private readonly TraineeImportValidator _sut;

    private static readonly string[] Headers =
        ["FirstName", "LastName", "BirthDate", "Gender", "Nationality", "PhoneNumber", "Email", "Branch", "NationalityCategory",
         "GuardianName", "ParentNumber", "Sports"];

    public TraineeImportValidatorTests()
    {
        _lookup.Setup(l => l.GetBranchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ImportNamedItem>
        {
            new(1, "Main", ["الرئيسي"], true),
            new(2, "Salmiya", ["السالمية"], true),
            new(3, "Old", [], false),
        });
        _lookup.Setup(l => l.GetNationalityCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ImportNamedItem>
        {
            new(1, "Citizen", ["مواطن"], true),
        });
        _lookup.Setup(l => l.GetSportsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ImportNamedItem>
        {
            new(10, "Football", ["كرة القدم"], true),
            new(11, "Swimming", ["السباحة"], true),
        });
        _lookup.Setup(l => l.GetExistingFamilyIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<int>());
        _lookup.Setup(l => l.GetExistingIdentifiersAsync(
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExistingTraineeIdentifiers(new HashSet<string> { "+96599999999" }, new HashSet<string>(), new HashSet<string>()));
        _lookup.Setup(l => l.GetFieldLimits()).Returns(new TraineeFieldLimits(50, 50, 20, 20, 20, 50, 200, 70, 50, 200));

        _branchAccess.SetupGet(b => b.IsRestricted).Returns(false);
        _branchAccess.SetupGet(b => b.AllowedBranchIds).Returns([]);
        _user.SetupGet(u => u.TenantId).Returns(Guid.NewGuid());
        _limits.Setup(l => l.GetAsync(It.IsAny<Guid>(), LimitedResources.Trainees, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveLimit(LimitedResources.Trainees, null, LimitSource.Unlimited, 0, null));

        var phone = new Mock<IPhoneNumberNormalizer>();
        phone.Setup(p => p.NormalizeAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string? p, CancellationToken _) => p is null ? null : "+965" + p.TrimStart('0'));

        _sut = new TraineeImportValidator(
            _lookup.Object, phone.Object, new InlineValidator<CreateTraineeCommand>(), new KeyLocalizer(),
            _branchAccess.Object, _limits.Object, _user.Object);
    }

    private static ImportRawRow Row(int number, params (string Header, string? Value)[] overrides)
    {
        var cells = new Dictionary<string, string?>
        {
            ["FirstName"] = "Ahmed",
            ["LastName"] = "Salem",
            ["BirthDate"] = "1995-04-10",
            ["Gender"] = "Male",
            ["Nationality"] = "Kuwaiti",
            ["PhoneNumber"] = $"5000{number:D4}",
            ["Email"] = $"a{number}@example.com",
            ["Branch"] = "Main",
            ["NationalityCategory"] = "Citizen",
            ["GuardianName"] = null,
            ["ParentNumber"] = null,
            ["Sports"] = null,
        };
        foreach (var (h, v) in overrides) cells[h] = v;
        return new ImportRawRow(number, cells);
    }

    private async Task<TraineeImportReport> Validate(params ImportRawRow[] rows)
        => (await _sut.ValidateAsync(rows, Headers, CancellationToken.None)).Report;

    [Fact]
    public async Task ValidRow_ResolvesNamesNotIds_AndIsValid()
    {
        var result = await _sut.ValidateAsync(
            [Row(2, ("Branch", "salmiya"), ("Sports", "Football | السباحة"))], Headers, CancellationToken.None);

        result.Report.ValidRows.Should().Be(1);
        var command = result.ValidRows.Single().Command;
        command.BranchId.Should().Be(2);
        command.SportIds.Should().BeEquivalentTo([10, 11]);
    }

    [Fact]
    public async Task MissingRequiredColumn_IsAFileError_AndNothingIsValid()
    {
        var result = await _sut.ValidateAsync([Row(2)], Headers.Where(h => h != "Email").ToList(), CancellationToken.None);

        result.Report.FileErrors.Should().ContainSingle(e => e == "import.file.missingColumns");
        result.ValidRows.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportedDisplayLabels_EnglishAndArabic_AreAcceptedAsHeaders()
    {
        var headers = new[] { "الاسم الأول", "Last Name", "Birth Date", "Gender", "Nationality", "Phone", "Email", "Branch", "Nationality Category" };
        var row = new ImportRawRow(2, new Dictionary<string, string?>
        {
            ["الاسم الأول"] = "Ahmed", ["Last Name"] = "Salem", ["Birth Date"] = "10/04/1995", ["Gender"] = "ذكر",
            ["Nationality"] = "كويتي", ["Phone"] = "50001234", ["Email"] = "x@example.com", ["Branch"] = "الرئيسي",
            ["Nationality Category"] = "مواطن",
        });

        var report = (await _sut.ValidateAsync([row], headers, CancellationToken.None)).Report;

        report.FileErrors.Should().BeEmpty();
        report.ValidRows.Should().Be(1);
    }

    [Fact]
    public async Task UnknownBranch_IsReportedAgainstTheBranchColumn()
    {
        var report = await Validate(Row(2, ("Branch", "Hawally")));

        report.Rows.Single().Errors.Should().ContainSingle(e => e.Message == "import.row.unknownBranch" && e.Value == "Hawally");
    }

    [Fact]
    public async Task DeactivatedBranch_IsRejected()
    {
        var report = await Validate(Row(2, ("Branch", "Old")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.inactiveBranch");
    }

    [Fact]
    public async Task PhoneAlreadyInDatabase_IsRejectedBeforeSaving()
    {
        var report = await Validate(Row(2, ("PhoneNumber", "99999999")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.phoneExists");
    }

    [Fact]
    public async Task SamePhoneTwiceInFile_FlagsTheSecondRow()
    {
        var report = await Validate(Row(2, ("PhoneNumber", "51112222")), Row(3, ("PhoneNumber", "051112222")));

        report.Rows[0].Status.Should().Be(ImportRowStatus.Valid);
        report.Rows[1].Errors.Should().Contain(e => e.Message == "import.row.duplicateInFile");
    }

    [Fact]
    public async Task ValueLongerThanTheDatabaseColumn_IsRejected()
    {
        var report = await Validate(Row(2, ("FirstName", new string('A', 51))));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.tooLong");
    }

    [Fact]
    public async Task UnderEighteen_WithoutGuardian_IsRejected()
    {
        var birth = DateTime.UtcNow.AddYears(-10).ToString("yyyy-MM-dd");
        var report = await Validate(Row(2, ("BirthDate", birth)));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.guardianRequired");
    }

    [Fact]
    public async Task ArabicIndicDigitsInDate_AreUnderstood()
    {
        var report = await Validate(Row(2, ("BirthDate", "١٩٩٥-٠٤-١٠")));

        report.ValidRows.Should().Be(1);
    }

    [Fact]
    public async Task InvalidDate_IsReportedWithTheValue()
    {
        var report = await Validate(Row(2, ("BirthDate", "31/31/2020")));

        report.Rows.Single().Errors.Should().ContainSingle(e => e.Message == "import.row.invalidDate" && e.Value == "31/31/2020");
    }

    [Fact]
    public async Task PlanLimit_RowsBeyondTheRemainingRoom_AreFlagged()
    {
        _limits.Setup(l => l.GetAsync(It.IsAny<Guid>(), LimitedResources.Trainees, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveLimit(LimitedResources.Trainees, 10, LimitSource.Plan, 9, null));

        var report = await Validate(Row(2), Row(3));

        report.ValidRows.Should().Be(1);
        report.Rows[1].Errors.Should().Contain(e => e.Message == "import.row.planLimit");
    }

    [Fact]
    public async Task ArabicFirstNameAtBranchTwo_WouldOverflowTheTraineeId_IsCaughtUpFront()
    {
        var report = await Validate(Row(2, ("FirstName", "أحمد"), ("Branch", "Salmiya")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.idOverflow");
    }
}
