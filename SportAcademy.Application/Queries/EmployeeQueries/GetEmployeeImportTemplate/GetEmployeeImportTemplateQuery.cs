using MediatR;
using SportAcademy.Application.Common.CsvImport;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Regional;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.DTOs.ImportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using Cols = SportAcademy.Application.Common.EmployeeImport.EmployeeImportColumns;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetEmployeeImportTemplate
{
    public record GetEmployeeImportTemplateQuery : IRequest<Result<EmployeeImportTemplateDto>>, IRequiresFeature
    {
        public string FeatureKey => "employee-management";
    }

    // Built from this academy's own data - its real branch NAMES (never ids), and a phone number
    // and national ID valid for its country - so the sample rows import as they are.
    public class GetEmployeeImportTemplateQueryHandler
        : IRequestHandler<GetEmployeeImportTemplateQuery, Result<EmployeeImportTemplateDto>>
    {
        private readonly IEmployeeImportLookup _lookup;
        private readonly ILocalizationService _localizer;
        private readonly ICurrentLanguageProvider _language;
        private readonly ITenantSettingsCountryReader _countryReader;
        private readonly IBranchAccessProvider _branchAccess;

        public GetEmployeeImportTemplateQueryHandler(
            IEmployeeImportLookup lookup,
            ILocalizationService localizer,
            ICurrentLanguageProvider language,
            ITenantSettingsCountryReader countryReader,
            IBranchAccessProvider branchAccess)
        {
            _lookup = lookup;
            _localizer = localizer;
            _language = language;
            _countryReader = countryReader;
            _branchAccess = branchAccess;
        }

        public async Task<Result<EmployeeImportTemplateDto>> Handle(GetEmployeeImportTemplateQuery request, CancellationToken ct)
        {
            var isArabic = _language.Language == "ar";
            var branches = (await _lookup.GetBranchesAsync(ct))
                .Where(b => b.IsActive && (!_branchAccess.IsRestricted || _branchAccess.AllowedBranchIds.Contains(b.Id)))
                .ToList();

            // Names shown in the user's language where a translation exists; the importer accepts
            // either language regardless.
            static string Shown(ImportNamedItem item, bool arabic)
                => arabic ? item.AlternateNames.FirstOrDefault(n => n.Any(c => c is >= '؀' and <= 'ۿ')) ?? item.Name : item.Name;

            var branchNames = branches.Select(b => Shown(b, isArabic)).ToList();
            var positions = Enum.GetValues<Position>().Select(p => _localizer.Label(p)).ToList();
            var genders = Enum.GetValues<Gender>().Select(g => _localizer.Label(g)).ToList();
            var nationalities = Enum.GetValues<Nationality>().Select(n => _localizer.Label(n)).ToList();

            var country = await _countryReader.GetCountryAsync(ct);
            var phones = ImportSamplePhones.For(country, 2);

            var columns = Cols.All.Select(c => new ImportColumnInfo(
                Key: c.Key,
                Label: _localizer[$"import.column.{c.Key}"],
                Required: c.Required,
                Description: _localizer[$"employeeImport.help.{c.Key}"],
                Example: string.Empty,
                AllowedValues: c.Key switch
                {
                    Cols.Gender => genders,
                    Cols.Branch => branchNames,
                    Cols.Position => positions,
                    _ => null,
                })).ToList();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var managerBirth = new DateOnly(today.Year - 34, 3, 14);
            var coachBirth = new DateOnly(today.Year - 27, 9, 2);

            var manager = new Dictionary<string, string>
            {
                [Cols.FirstName] = "Yousef",
                [Cols.LastName] = "Al-Mutairi",
                [Cols.BirthDate] = managerBirth.ToString("yyyy-MM-dd"),
                [Cols.Gender] = _localizer.Label(Gender.Male),
                [Cols.Nationality] = nationalities.FirstOrDefault() ?? string.Empty,
                [Cols.PhoneNumber] = phones[0],
                [Cols.Email] = "yousef.almutairi@example.com",
                [Cols.Ssn] = SampleNationalId(country, managerBirth, 1),
                [Cols.Branch] = branchNames.FirstOrDefault() ?? string.Empty,
                [Cols.Position] = _localizer.Label(Position.Manager),
                [Cols.Salary] = "1200",
                [Cols.Street] = isArabic ? "شارع الخليج" : "Gulf Road",
                [Cols.City] = isArabic ? "الكويت" : "Kuwait City",
                [Cols.SecondPhoneNumber] = string.Empty,
            };
            var coach = new Dictionary<string, string>(manager)
            {
                [Cols.FirstName] = "Mona",
                [Cols.LastName] = "Al-Kandari",
                [Cols.BirthDate] = coachBirth.ToString("yyyy-MM-dd"),
                [Cols.Gender] = _localizer.Label(Gender.Female),
                [Cols.PhoneNumber] = phones[1],
                [Cols.Email] = "mona.alkandari@example.com",
                [Cols.Ssn] = SampleNationalId(country, coachBirth, 2),
                [Cols.Position] = _localizer.Label(Position.Coach),
                [Cols.Salary] = "650",
            };

            return Result<EmployeeImportTemplateDto>.Success(new EmployeeImportTemplateDto(
                columns, [manager, coach], branchNames, positions, genders, nationalities),
                OperationType.Get.ToString());
        }

        // Every supported country's national ID starts with a century digit (2 = 1900s, 3 = 2000s)
        // and the birth date as yyMMdd (CountryRegionalRegistry); the rest is padded to the
        // country's length (12 for the generic 5-20 digit fallback).
        private static string SampleNationalId(string countryIso, DateOnly birthDate, int sequence)
        {
            var length = CountryRegionalRegistry.GetNationalIdRule(countryIso).FixedLength ?? 12;
            var prefix = (birthDate.Year > 1999 ? "3" : "2") + birthDate.ToString("yyMMdd");
            if (prefix.Length >= length) return prefix[..length];
            return prefix + sequence.ToString().PadLeft(length - prefix.Length, '0');
        }
    }
}
