using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Regional;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.DTOs.ImportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.EmployeeQueries.GetEmployeeImportTemplate;
using SportAcademy.Application.Services.EmployeeImport;
using SportAcademy.Application.Validators.EmployeeValidators;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Tests.Application.Validators;

namespace SportAcademy.Tests.Application.Services;

public class EmployeeImportValidatorTests
{
    // Returns the message KEY (so tests assert which rule fired, not wording) and real column /
    // enum labels for the header index and the name matchers.
    private sealed class KeyLocalizer : ILocalizationService
    {
        private static readonly Dictionary<string, string> Labels = new()
        {
            ["en:import.column.FirstName"] = "First Name",
            ["ar:import.column.FirstName"] = "الاسم الأول",
            ["en:import.column.Salary"] = "Salary",
            ["ar:import.column.Salary"] = "الراتب",
            ["en:import.column.Position"] = "Position",
            ["en:enum.Gender.Male"] = "Male",
            ["ar:enum.Gender.Male"] = "ذكر",
            ["en:enum.Gender.Female"] = "Female",
            ["en:enum.Nationality.Kuwaiti"] = "Kuwaiti",
            ["ar:enum.Nationality.Kuwaiti"] = "كويتي",
            ["en:enum.Position.Coach"] = "Coach",
            ["ar:enum.Position.Coach"] = "مدرب",
            ["en:enum.Position.Manager"] = "Manager",
        };

        public string this[string key, params object[] args] => key;
        public string GetIn(string language, string key, params object[] args)
            => Labels.TryGetValue($"{language}:{key}", out var v) ? v : key;
        public bool Exists(string key) => false;
    }

    private readonly Mock<IEmployeeImportLookup> _lookup = new();
    private readonly Mock<IBranchAccessProvider> _branchAccess = new();
    private readonly Mock<IPhoneNumberNormalizer> _phone = new();
    private readonly EmployeeImportValidator _sut;

    private static readonly string[] Headers =
        ["FirstName", "LastName", "BirthDate", "Gender", "Nationality", "PhoneNumber", "Email", "SSN",
         "Branch", "Position", "Salary", "Street", "City", "SecondPhoneNumber"];

    public EmployeeImportValidatorTests()
    {
        _lookup.Setup(l => l.GetBranchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ImportNamedItem>
        {
            new(1, "Main", ["الرئيسي"], true),
            new(2, "Salmiya", ["السالمية"], true),
            new(3, "Old", [], false),
        });
        _lookup.Setup(l => l.GetExistingIdentifiersAsync(
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExistingEmployeeIdentifiers(new HashSet<string> { "+96599999999" }, new HashSet<string> { "290040599999" }));
        _lookup.Setup(l => l.GetFieldLimits()).Returns(new EmployeeFieldLimits(50, 50, 20, 20, 20, 200, 70, 50));

        _branchAccess.SetupGet(b => b.IsRestricted).Returns(false);
        _branchAccess.SetupGet(b => b.AllowedBranchIds).Returns([]);

        _phone.Setup(p => p.NormalizeAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string? p, CancellationToken _) => p is null ? null : "+965" + p.TrimStart('0'));

        // The real create-employee rules (Kuwait phone + civil ID checks), not a stub, so the
        // tests prove the import holds rows to exactly what the Add Employee form does.
        _sut = new EmployeeImportValidator(
            _lookup.Object, _phone.Object,
            new CreateEmployeeValidator(new RegionalValidationService(), new FixedCountryReader("KW")),
            new KeyLocalizer(), _branchAccess.Object);
    }

    // Born 1990-04-05 -> Kuwaiti civil ID "2" + "900405" + 5 digits.
    private static ImportRawRow Row(int number, params (string Header, string? Value)[] overrides)
    {
        var cells = new Dictionary<string, string?>
        {
            ["FirstName"] = "Yousef",
            ["LastName"] = "Mutairi",
            ["BirthDate"] = "1990-04-05",
            ["Gender"] = "Male",
            ["Nationality"] = "Kuwaiti",
            ["PhoneNumber"] = $"5000{number:D4}",
            ["Email"] = $"e{number}@example.com",
            ["SSN"] = $"29004051{number:D4}",
            ["Branch"] = "Main",
            ["Position"] = "Manager",
            ["Salary"] = "1200",
            ["Street"] = "Gulf Road",
            ["City"] = "Kuwait City",
            ["SecondPhoneNumber"] = null,
        };
        foreach (var (h, v) in overrides) cells[h] = v;
        return new ImportRawRow(number, cells);
    }

    private async Task<EmployeeImportReport> Validate(params ImportRawRow[] rows)
        => (await _sut.ValidateAsync(rows, Headers, CancellationToken.None)).Report;

    [Fact]
    public async Task ValidRow_ResolvesNamesAndLabels_IntoTheCreateCommand()
    {
        var result = await _sut.ValidateAsync(
            [Row(2, ("Branch", "السالمية"), ("Position", "مدرب"), ("Gender", "ذكر"), ("Salary", "1,250 KWD"))],
            Headers, CancellationToken.None);

        result.Report.ValidRows.Should().Be(1, string.Join("; ", result.Report.Rows.SelectMany(r => r.Errors).Select(e => $"{e.Column}: {e.Message}")));
        var command = result.ValidRows.Single().Command;
        command.BranchId.Should().Be(2);
        command.Position.Should().Be(Position.Coach);
        command.Gender.Should().Be(Gender.Male);
        command.Salary.Should().Be(1250m);
        command.Nationality.Should().Be(nameof(Nationality.Kuwaiti));
        command.CreateUserAccount.Should().BeFalse();
    }

    [Fact]
    public async Task MissingRequiredColumn_IsAFileError_AndNothingIsValid()
    {
        var result = await _sut.ValidateAsync([Row(2)], Headers.Where(h => h != "Salary").ToList(), CancellationToken.None);

        result.Report.FileErrors.Should().ContainSingle(e => e == "import.file.missingColumns");
        result.ValidRows.Should().BeEmpty();
    }

    [Fact]
    public async Task LocalizedHeaders_AreAccepted()
    {
        var headers = Headers.Select(h => h switch { "FirstName" => "الاسم الأول", "Salary" => "الراتب", _ => h }).ToArray();
        var cells = Row(2).Cells.ToDictionary(
            kv => kv.Key switch { "FirstName" => "الاسم الأول", "Salary" => "الراتب", _ => kv.Key }, kv => kv.Value);

        var report = (await _sut.ValidateAsync([new ImportRawRow(2, cells)], headers, CancellationToken.None)).Report;

        report.FileErrors.Should().BeEmpty();
        report.ValidRows.Should().Be(1);
    }

    [Fact]
    public async Task PhoneAlreadyBelongingToAnEmployee_IsRejectedBeforeSaving()
    {
        var report = await Validate(Row(2, ("PhoneNumber", "99999999")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "employeeImport.row.phoneExists");
    }

    [Fact]
    public async Task SsnAlreadyBelongingToAnEmployee_IsRejectedBeforeSaving()
    {
        var report = await Validate(Row(2, ("SSN", "290040599999")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "employeeImport.row.ssnExists");
    }

    [Fact]
    public async Task SameSsnTwiceInFile_FlagsTheSecondRow()
    {
        var report = await Validate(Row(2, ("SSN", "290040511111")), Row(3, ("SSN", "290040511111")));

        report.Rows[0].Status.Should().Be(ImportRowStatus.Valid);
        report.Rows[1].Errors.Should().Contain(e => e.Message == "import.row.duplicateInFile");
    }

    [Fact]
    public async Task UnknownPosition_IsReportedWithTheValue()
    {
        var report = await Validate(Row(2, ("Position", "Janitor")));

        report.Rows.Single().Errors.Should().ContainSingle(e => e.Message == "employeeImport.row.invalidPosition" && e.Value == "Janitor");
    }

    [Fact]
    public async Task NonNumericSalary_IsRejected()
    {
        var report = await Validate(Row(2, ("Salary", "a lot")));

        report.Rows.Single().Errors.Should().ContainSingle(e => e.Message == "employeeImport.row.invalidSalary");
    }

    [Fact]
    public async Task ZeroSalary_FailsTheCreateEmployeeRule()
    {
        var report = await Validate(Row(2, ("Salary", "0")));

        report.Rows.Single().Errors.Should().Contain(e => e.Column == "import.column.Salary");
    }

    [Fact]
    public async Task CivilIdNotMatchingTheBirthDate_IsRejected()
    {
        var report = await Validate(Row(2, ("SSN", "299123112345")));

        report.Rows.Single().Errors.Should().Contain(e => e.Column == "import.column.SSN");
    }

    [Fact]
    public async Task DeactivatedBranch_IsRejected()
    {
        var report = await Validate(Row(2, ("Branch", "Old")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "employeeImport.row.inactiveBranch");
    }

    [Fact]
    public async Task BranchOutsideTheCallersAccess_IsRejected()
    {
        _branchAccess.SetupGet(b => b.IsRestricted).Returns(true);
        _branchAccess.SetupGet(b => b.AllowedBranchIds).Returns([1]);

        var report = await Validate(Row(2, ("Branch", "Salmiya")));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.branchNoAccess");
    }

    [Fact]
    public async Task ValueLongerThanTheDatabaseColumn_IsRejected()
    {
        var report = await Validate(Row(2, ("Street", new string('A', 71))));

        report.Rows.Single().Errors.Should().Contain(e => e.Message == "import.row.tooLong");
    }

    [Theory]
    [InlineData("KW")]
    [InlineData("EG")]
    [InlineData("SA")]
    public async Task TemplateSampleRows_ImportAsTheyAre(string country)
    {
        var language = new Mock<ICurrentLanguageProvider>();
        language.SetupGet(l => l.Language).Returns("en");
        var template = await new GetEmployeeImportTemplateQueryHandler(
                _lookup.Object, new KeyLocalizer(), language.Object, new FixedCountryReader(country), _branchAccess.Object)
            .Handle(new GetEmployeeImportTemplateQuery(), CancellationToken.None);

        var phone = new Mock<IPhoneNumberNormalizer>();
        phone.Setup(p => p.NormalizeAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string? p, CancellationToken _) => p);
        var validator = new EmployeeImportValidator(
            _lookup.Object, phone.Object,
            new CreateEmployeeValidator(new RegionalValidationService(), new FixedCountryReader(country)),
            new KeyLocalizer(), _branchAccess.Object);

        var rows = template.Data!.SampleRows
            .Select((r, i) => new ImportRawRow(i + 2, r.ToDictionary(kv => kv.Key, kv => (string?)kv.Value)))
            .ToList();
        var report = (await validator.ValidateAsync(rows, Headers, CancellationToken.None)).Report;

        report.ValidRows.Should().Be(rows.Count,
            string.Join("; ", report.Rows.SelectMany(r => r.Errors).Select(e => $"{e.Column} '{e.Value}': {e.Message}")));
    }
}
