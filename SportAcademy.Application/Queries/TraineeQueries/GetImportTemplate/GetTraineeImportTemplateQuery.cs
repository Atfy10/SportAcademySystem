using MediatR;
using PhoneNumbers;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using Cols = SportAcademy.Application.Common.TraineeImport.TraineeImportColumns;

namespace SportAcademy.Application.Queries.TraineeQueries.GetImportTemplate
{
    public record GetTraineeImportTemplateQuery : IRequest<Result<TraineeImportTemplateDto>>, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
    }

    // The template is built from this academy's own data - its real branch, category and sport
    // NAMES (never ids), a phone number valid for its country, and a child row that shows the
    // guardian columns filled in - so a user who copies the sample rows gets rows that import.
    public class GetTraineeImportTemplateQueryHandler
        : IRequestHandler<GetTraineeImportTemplateQuery, Result<TraineeImportTemplateDto>>
    {
        private readonly ITraineeImportLookup _lookup;
        private readonly ILocalizationService _localizer;
        private readonly ICurrentLanguageProvider _language;
        private readonly ITenantSettingsCountryReader _countryReader;
        private readonly IEffectiveLimitService _limits;
        private readonly IUserContextService _userContext;
        private readonly IBranchAccessProvider _branchAccess;

        public GetTraineeImportTemplateQueryHandler(
            ITraineeImportLookup lookup,
            ILocalizationService localizer,
            ICurrentLanguageProvider language,
            ITenantSettingsCountryReader countryReader,
            IEffectiveLimitService limits,
            IUserContextService userContext,
            IBranchAccessProvider branchAccess)
        {
            _lookup = lookup;
            _localizer = localizer;
            _language = language;
            _countryReader = countryReader;
            _limits = limits;
            _userContext = userContext;
            _branchAccess = branchAccess;
        }

        public async Task<Result<TraineeImportTemplateDto>> Handle(GetTraineeImportTemplateQuery request, CancellationToken ct)
        {
            var isArabic = _language.Language == "ar";
            var branches = (await _lookup.GetBranchesAsync(ct))
                .Where(b => b.IsActive && (!_branchAccess.IsRestricted || _branchAccess.AllowedBranchIds.Contains(b.Id)))
                .ToList();
            var categories = await _lookup.GetNationalityCategoriesAsync(ct);
            var sports = (await _lookup.GetSportsAsync(ct)).Where(s => s.IsActive).ToList();

            // Names shown in the user's language where a translation exists; the importer accepts
            // either language regardless.
            static string Shown(ImportNamedItem item, bool arabic)
                => arabic ? item.AlternateNames.FirstOrDefault(n => n.Any(c => c is >= '؀' and <= 'ۿ')) ?? item.Name : item.Name;

            var branchNames = branches.Select(b => Shown(b, isArabic)).ToList();
            var categoryNames = categories.Select(c => Shown(c, isArabic)).ToList();
            var sportNames = sports.Select(s => Shown(s, isArabic)).ToList();
            var genders = Enum.GetValues<Gender>().Select(g => _localizer.Label(g)).ToList();
            var nationalities = Enum.GetValues<Nationality>().Select(n => _localizer.Label(n)).ToList();

            var country = await _countryReader.GetCountryAsync(ct);
            var phones = SamplePhones(country);

            string L(string key) => _localizer[$"import.column.{key}"];
            string H(string key) => _localizer[$"import.help.{key}"];

            var columns = Cols.All.Select(c => new TraineeImportColumnInfo(
                Key: c.Key,
                Label: L(c.Key),
                Required: c.Required,
                Description: H(c.Key),
                Example: string.Empty,
                AllowedValues: c.Key switch
                {
                    Cols.Gender => genders,
                    Cols.Branch => branchNames,
                    Cols.NationalityCategory => categoryNames,
                    Cols.Sports => sportNames,
                    _ => null,
                })).ToList();

            var today = DateTime.UtcNow;
            var adult = new Dictionary<string, string>
            {
                [Cols.FirstName] = "Ahmed",
                [Cols.LastName] = "Al-Salem",
                [Cols.BirthDate] = today.AddYears(-25).ToString("yyyy-MM-dd"),
                [Cols.Gender] = _localizer.Label(Gender.Male),
                [Cols.Nationality] = nationalities.FirstOrDefault() ?? string.Empty,
                [Cols.PhoneNumber] = phones.Adult,
                [Cols.Email] = "ahmed.alsalem@example.com",
                [Cols.Branch] = branchNames.FirstOrDefault() ?? string.Empty,
                [Cols.NationalityCategory] = categoryNames.FirstOrDefault() ?? string.Empty,
                [Cols.Sports] = string.Join(" | ", sportNames.Take(2)),
                [Cols.Ssn] = string.Empty,
                [Cols.GuardianName] = string.Empty,
                [Cols.ParentNumber] = string.Empty,
                [Cols.Street] = string.Empty,
                [Cols.City] = string.Empty,
                [Cols.FamilyId] = string.Empty,
                [Cols.MedicalConditions] = string.Empty,
            };
            var child = new Dictionary<string, string>(adult)
            {
                [Cols.FirstName] = "Sara",
                [Cols.BirthDate] = today.AddYears(-10).ToString("yyyy-MM-dd"),
                [Cols.Gender] = _localizer.Label(Gender.Female),
                [Cols.PhoneNumber] = phones.Child,
                [Cols.Email] = "sara.alsalem@example.com",
                [Cols.Sports] = sportNames.FirstOrDefault() ?? string.Empty,
                [Cols.GuardianName] = "Khaled Al-Salem",
                [Cols.ParentNumber] = phones.Parent,
                [Cols.MedicalConditions] = isArabic ? "ربو" : "Asthma",
            };

            int? remaining = null;
            if (_userContext.TenantId is { } tenantId)
            {
                var limit = await _limits.GetAsync(tenantId, LimitedResources.Trainees, ct);
                if (limit.MaxCount is { } max) remaining = Math.Max(0, max - limit.Used);
            }

            return Result<TraineeImportTemplateDto>.Success(new TraineeImportTemplateDto(
                columns, [adult, child], branchNames, categoryNames, sportNames, genders, nationalities, remaining),
                OperationType.Get.ToString());
        }

        // Three distinct, valid mobile numbers for the tenant's country (libphonenumber's own
        // example number, varied in its last digit), as plain national digits - a leading "+"
        // is the first thing Excel strips when the file is opened and saved again.
        private static (string Adult, string Child, string Parent) SamplePhones(string countryIso)
        {
            var util = PhoneNumberUtil.GetInstance();
            var example = util.GetExampleNumberForType(countryIso, PhoneNumberType.MOBILE)
                ?? util.GetExampleNumberForType("KW", PhoneNumberType.MOBILE);
            if (example is null) return (string.Empty, string.Empty, string.Empty);

            string Variant(int delta)
            {
                var national = example.NationalNumber;
                var candidate = new PhoneNumber.Builder()
                    .SetCountryCode(example.CountryCode)
                    .SetNationalNumber(national - (national % 10) + (ulong)((national % 10 + (ulong)delta) % 10))
                    .Build();
                var chosen = util.IsValidNumber(candidate) ? candidate : example;
                return new string(util.Format(chosen, PhoneNumberFormat.NATIONAL).Where(char.IsDigit).ToArray());
            }

            return (Variant(0), Variant(1), Variant(2));
        }
    }
}
