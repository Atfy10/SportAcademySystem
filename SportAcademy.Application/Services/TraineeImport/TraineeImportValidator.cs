using FluentValidation;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Common.CsvImport;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.TraineeImport;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.DTOs.ImportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using System.Globalization;
using Cols = SportAcademy.Application.Common.TraineeImport.TraineeImportColumns;

namespace SportAcademy.Application.Services.TraineeImport
{
    public record TraineeImportValidationResult(
        TraineeImportReport Report,
        IReadOnlyList<(int RowNumber, CreateTraineeCommand Command)> ValidRows);

    // Checks every row of a trainee CSV against everything that could make saving it fail -
    // required values, formats, the database's own column lengths, references by name or id,
    // branch access, the create-trainee business rules (same FluentValidation validator the
    // create form goes through), uniqueness against the database AND within the file, and the
    // plan's trainee limit - and reports each problem against its row, column and value. Nothing
    // is written. The import only ever saves rows this has passed, so the database is never the
    // one to reject a row with a generic error.
    public class TraineeImportValidator
    {
        public const int MaxRows = 2000;

        private readonly ITraineeImportLookup _lookup;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly IValidator<CreateTraineeCommand> _createValidator;
        private readonly ILocalizationService _localizer;
        private readonly IBranchAccessProvider _branchAccess;
        private readonly IEffectiveLimitService _limits;
        private readonly IUserContextService _userContext;

        public TraineeImportValidator(
            ITraineeImportLookup lookup,
            IPhoneNumberNormalizer phoneNormalizer,
            IValidator<CreateTraineeCommand> createValidator,
            ILocalizationService localizer,
            IBranchAccessProvider branchAccess,
            IEffectiveLimitService limits,
            IUserContextService userContext)
        {
            _lookup = lookup;
            _phoneNormalizer = phoneNormalizer;
            _createValidator = createValidator;
            _localizer = localizer;
            _branchAccess = branchAccess;
            _limits = limits;
            _userContext = userContext;
        }

        // Header text -> canonical column key, recognising the canonical names, every alias and
        // the localized column labels in both languages.
        public Dictionary<string, string> BuildHeaderIndex()
        {
            var index = new Dictionary<string, string>();
            foreach (var col in Cols.All)
            {
                foreach (var name in col.Aliases
                    .Append(col.Key)
                    .Append(_localizer.GetIn("en", $"import.column.{col.Key}"))
                    .Append(_localizer.GetIn("ar", $"import.column.{col.Key}")))
                {
                    var key = Cols.Normalize(name);
                    if (key.Length > 0) index.TryAdd(key, col.Key);
                }
            }
            return index;
        }

        public string ColumnLabel(string key) => _localizer[$"import.column.{key}"];

        public async Task<TraineeImportValidationResult> ValidateAsync(
            IReadOnlyList<ImportRawRow> rawRows, IReadOnlyList<string> headers, CancellationToken ct)
        {
            var fileErrors = new List<string>();
            var headerIndex = BuildHeaderIndex();

            // Which file header feeds which column (first one wins if a column appears twice).
            var headerFor = new Dictionary<string, string>();
            var unknown = new List<string>();
            foreach (var header in headers)
            {
                if (string.IsNullOrWhiteSpace(header)) continue;
                if (headerIndex.TryGetValue(Cols.Normalize(header), out var key))
                    headerFor.TryAdd(key, header);
                else
                    unknown.Add(header);
            }

            var missing = Cols.All.Where(c => c.Required && !headerFor.ContainsKey(c.Key)).Select(c => ColumnLabel(c.Key)).ToList();
            if (missing.Count > 0)
                fileErrors.Add(_localizer["import.file.missingColumns", string.Join(", ", missing)]);
            if (rawRows.Count == 0)
                fileErrors.Add(_localizer["import.file.empty"]);
            if (rawRows.Count > MaxRows)
                fileErrors.Add(_localizer["import.file.tooManyRows", MaxRows]);

            if (fileErrors.Count > 0)
                return new TraineeImportValidationResult(
                    new TraineeImportReport(rawRows.Count, 0, rawRows.Count, 0, 0, false, fileErrors, unknown, []), []);

            var branches = await _lookup.GetBranchesAsync(ct);
            var categories = await _lookup.GetNationalityCategoriesAsync(ct);
            var sports = await _lookup.GetSportsAsync(ct);
            var limits = _lookup.GetFieldLimits();
            var genders = ImportValueParser.EnumMatcher<Gender>(_localizer);
            var nationalities = ImportValueParser.EnumMatcher<Nationality>(_localizer);

            var parsed = new List<ParsedRow>(rawRows.Count);
            foreach (var raw in rawRows)
            {
                ct.ThrowIfCancellationRequested();
                parsed.Add(await ParseRowAsync(raw, headerFor, branches, categories, sports, genders, nationalities, limits, ct));
            }

            // ── Family ids must exist ──────────────────────────────────────────────────────
            var familyIds = parsed.Where(p => p.FamilyId > 0).Select(p => p.FamilyId).ToList();
            var existingFamilies = await _lookup.GetExistingFamilyIdsAsync(familyIds, ct);
            foreach (var p in parsed.Where(p => p.FamilyId > 0 && !existingFamilies.Contains(p.FamilyId)))
                p.AddError(ColumnLabel(Cols.FamilyId), p.FamilyId.ToString(), _localizer["import.row.unknownFamily", p.FamilyId]);

            // ── Uniqueness against the database ────────────────────────────────────────────
            var existing = await _lookup.GetExistingIdentifiersAsync(
                parsed.Select(p => p.NormalizedPhone ?? ""),
                parsed.Select(p => p.Ssn ?? ""),
                parsed.Select(p => p.Email ?? ""), ct);

            foreach (var p in parsed)
            {
                if (p.NormalizedPhone is { } phone && existing.Phones.Contains(phone))
                    p.AddError(ColumnLabel(Cols.PhoneNumber), p.PhoneRaw, _localizer["import.row.phoneExists", p.PhoneRaw ?? phone]);
                if (!string.IsNullOrEmpty(p.Ssn) && existing.Ssns.Contains(p.Ssn))
                    p.AddError(ColumnLabel(Cols.Ssn), p.Ssn, _localizer["import.row.ssnExists"]);
                if (!string.IsNullOrEmpty(p.Email) && existing.Emails.Contains(p.Email.ToLowerInvariant()))
                    p.AddError(ColumnLabel(Cols.Email), p.Email, _localizer["import.row.emailExists", p.Email]);
            }

            // ── Uniqueness within the file ─────────────────────────────────────────────────
            FlagInFileDuplicates(parsed, p => p.NormalizedPhone, Cols.PhoneNumber, p => p.PhoneRaw);
            FlagInFileDuplicates(parsed, p => string.IsNullOrEmpty(p.Ssn) ? null : p.Ssn, Cols.Ssn, p => p.Ssn);
            FlagInFileDuplicates(parsed, p => p.Email?.ToLowerInvariant(), Cols.Email, p => p.Email);

            // ── Plan limit: only as many new trainees as the plan has room for ─────────────
            if (_userContext.TenantId is { } tenantId)
            {
                var limit = await _limits.GetAsync(tenantId, LimitedResources.Trainees, ct);
                if (limit.MaxCount is { } max)
                {
                    var room = Math.Max(0, max - limit.Used);
                    foreach (var p in parsed.Where(p => p.Errors.Count == 0).Skip(room))
                        p.AddError(string.Empty, null, _localizer["import.row.planLimit", room]);
                }
            }

            var rows = parsed.Select(p => new TraineeImportRowReport(
                p.RowNumber,
                p.DisplayName,
                p.Errors.Count == 0 ? ImportRowStatus.Valid : ImportRowStatus.Invalid,
                p.Errors)).ToList();

            var valid = parsed.Where(p => p.Errors.Count == 0 && p.Command is not null)
                .Select(p => (p.RowNumber, p.Command!))
                .ToList();

            var report = new TraineeImportReport(
                TotalRows: parsed.Count,
                ValidRows: valid.Count,
                InvalidRows: parsed.Count - valid.Count,
                ImportedRows: 0,
                FailedRows: 0,
                Committed: false,
                FileErrors: [],
                UnknownColumns: unknown,
                Rows: rows);

            return new TraineeImportValidationResult(report, valid);
        }

        private void FlagInFileDuplicates(
            List<ParsedRow> rows, Func<ParsedRow, string?> keyOf, string column, Func<ParsedRow, string?> shown)
        {
            var firstSeen = new Dictionary<string, int>();
            foreach (var p in rows)
            {
                var key = keyOf(p);
                if (string.IsNullOrEmpty(key)) continue;
                if (firstSeen.TryGetValue(key, out var firstRow))
                    p.AddError(ColumnLabel(column), shown(p), _localizer["import.row.duplicateInFile", ColumnLabel(column), firstRow]);
                else
                    firstSeen[key] = p.RowNumber;
            }
        }

        private sealed class ParsedRow
        {
            public int RowNumber { get; init; }
            public string? DisplayName { get; set; }
            public string? PhoneRaw { get; set; }
            public string? NormalizedPhone { get; set; }
            public string? Ssn { get; set; }
            public string? Email { get; set; }
            public int FamilyId { get; set; }
            public CreateTraineeCommand? Command { get; set; }
            public List<ImportCellError> Errors { get; } = [];

            public void AddError(string column, string? value, string message)
            {
                // One message per column is enough - the first problem found is the one to fix.
                if (column.Length > 0 && Errors.Any(e => e.Column == column)) return;
                Errors.Add(new ImportCellError(column, value, message));
            }
        }

        private async Task<ParsedRow> ParseRowAsync(
            ImportRawRow raw,
            Dictionary<string, string> headerFor,
            IReadOnlyList<ImportNamedItem> branches,
            IReadOnlyList<ImportNamedItem> categories,
            IReadOnlyList<ImportNamedItem> sports,
            Dictionary<string, Gender> genders,
            Dictionary<string, Nationality> nationalities,
            TraineeFieldLimits limits,
            CancellationToken ct)
        {
            var row = new ParsedRow { RowNumber = raw.RowNumber };

            string? Cell(string key)
            {
                if (!headerFor.TryGetValue(key, out var header)) return null;
                if (!raw.Cells.TryGetValue(header, out var v) || string.IsNullOrWhiteSpace(v)) return null;
                return v.Trim();
            }

            string? Required(string key)
            {
                var v = Cell(key);
                if (v is null) row.AddError(ColumnLabel(key), null, _localizer["import.row.required", ColumnLabel(key)]);
                return v;
            }

            string? Limited(string key, string? value, int max)
            {
                if (value is not null && value.Length > max)
                {
                    row.AddError(ColumnLabel(key), value, _localizer["import.row.tooLong", ColumnLabel(key), max, value.Length]);
                    return null;
                }
                return value;
            }

            var firstName = Limited(Cols.FirstName, Required(Cols.FirstName), limits.FirstName);
            var lastName = Limited(Cols.LastName, Required(Cols.LastName), limits.LastName);
            row.DisplayName = string.Join(" ", new[] { Cell(Cols.FirstName), Cell(Cols.LastName) }.Where(s => s is not null));

            // Birth date
            DateOnly birthDate = default;
            var birthRaw = Required(Cols.BirthDate);
            if (birthRaw is not null)
            {
                if (ImportValueParser.TryParseDate(birthRaw, out birthDate))
                {
                    if (birthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                        row.AddError(ColumnLabel(Cols.BirthDate), birthRaw, _localizer["import.row.futureDate"]);
                }
                else
                {
                    row.AddError(ColumnLabel(Cols.BirthDate), birthRaw, _localizer["import.row.invalidDate", birthRaw]);
                }
            }

            // Gender / nationality
            Gender gender = default;
            var genderRaw = Required(Cols.Gender);
            if (genderRaw is not null && !genders.TryGetValue(Cols.Normalize(genderRaw), out gender))
                row.AddError(ColumnLabel(Cols.Gender), genderRaw,
                    _localizer["import.row.invalidGender", genderRaw, string.Join(" / ", Enum.GetValues<Gender>().Select(g => _localizer.Label(g)))]);

            Nationality nationality = default;
            var nationalityRaw = Required(Cols.Nationality);
            if (nationalityRaw is not null && !nationalities.TryGetValue(Cols.Normalize(nationalityRaw), out nationality))
                row.AddError(ColumnLabel(Cols.Nationality), nationalityRaw, _localizer["import.row.invalidNationality", nationalityRaw]);

            // Contacts
            var phoneRaw = Limited(Cols.PhoneNumber, Required(Cols.PhoneNumber), 30);
            row.PhoneRaw = phoneRaw;
            if (phoneRaw is not null)
            {
                row.NormalizedPhone = await _phoneNormalizer.NormalizeAsync(DigitNormalizer.ToAscii(phoneRaw), ct);
                if (row.NormalizedPhone is { Length: var len } && len > limits.PhoneNumber)
                    row.AddError(ColumnLabel(Cols.PhoneNumber), phoneRaw, _localizer["import.row.tooLong", ColumnLabel(Cols.PhoneNumber), limits.PhoneNumber, len]);
            }

            var email = Limited(Cols.Email, Required(Cols.Email), limits.Email);
            row.Email = email;

            var ssnRaw = Cell(Cols.Ssn);
            var ssn = ssnRaw is null ? null : Limited(Cols.Ssn, DigitNormalizer.ToAscii(ssnRaw).Replace(" ", ""), limits.Ssn);
            row.Ssn = ssn;

            var guardian = Limited(Cols.GuardianName, Cell(Cols.GuardianName), limits.GuardianName);
            var parentRaw = Cell(Cols.ParentNumber);
            string? parentNumber = null;
            if (parentRaw is not null)
            {
                parentNumber = DigitNormalizer.ToAscii(parentRaw);
                var normalizedParent = await _phoneNormalizer.NormalizeAsync(parentNumber, ct);
                if (normalizedParent is { Length: var plen } && plen > limits.ParentNumber)
                    row.AddError(ColumnLabel(Cols.ParentNumber), parentRaw, _localizer["import.row.tooLong", ColumnLabel(Cols.ParentNumber), limits.ParentNumber, plen]);
            }

            var street = Limited(Cols.Street, Cell(Cols.Street), limits.Street);
            var city = Limited(Cols.City, Cell(Cols.City), limits.City);

            // Branch
            int branchId = 0;
            var branchRaw = Required(Cols.Branch);
            if (branchRaw is not null)
            {
                var branch = ImportValueParser.Resolve(branches, branchRaw);
                if (branch is null)
                    row.AddError(ColumnLabel(Cols.Branch), branchRaw,
                        _localizer["import.row.unknownBranch", branchRaw, string.Join(", ", branches.Where(b => b.IsActive).Select(b => b.Name))]);
                else if (!branch.IsActive)
                    row.AddError(ColumnLabel(Cols.Branch), branchRaw, _localizer["import.row.inactiveBranch", branch.Name]);
                else if (_branchAccess.IsRestricted && !_branchAccess.AllowedBranchIds.Contains(branch.Id))
                    row.AddError(ColumnLabel(Cols.Branch), branchRaw, _localizer["import.row.branchNoAccess", branch.Name]);
                else
                    branchId = branch.Id;
            }

            // Nationality category
            int categoryId = 0;
            var categoryRaw = Required(Cols.NationalityCategory);
            if (categoryRaw is not null)
            {
                var category = ImportValueParser.Resolve(categories, categoryRaw);
                if (category is null)
                    row.AddError(ColumnLabel(Cols.NationalityCategory), categoryRaw,
                        _localizer["import.row.unknownCategory", categoryRaw, string.Join(", ", categories.Select(c => c.Name))]);
                else
                    categoryId = category.Id;
            }

            // Sports (optional, several)
            var sportIds = new HashSet<int>();
            var sportsRaw = Cell(Cols.Sports);
            if (sportsRaw is not null)
            {
                foreach (var part in ImportValueParser.SplitList(sportsRaw))
                {
                    var sport = ImportValueParser.Resolve(sports, part);
                    if (sport is null)
                    {
                        row.AddError(ColumnLabel(Cols.Sports), part,
                            _localizer["import.row.unknownSport", part, string.Join(", ", sports.Where(s => s.IsActive).Select(s => s.Name))]);
                        break;
                    }
                    if (!sport.IsActive)
                    {
                        row.AddError(ColumnLabel(Cols.Sports), part, _localizer["import.row.inactiveSport", sport.Name]);
                        break;
                    }
                    sportIds.Add(sport.Id);
                }
            }

            // Family (optional)
            var familyRaw = Cell(Cols.FamilyId);
            if (familyRaw is not null)
            {
                if (int.TryParse(DigitNormalizer.ToAscii(familyRaw), NumberStyles.Integer, CultureInfo.InvariantCulture, out var familyId) && familyId >= 0)
                    row.FamilyId = familyId;
                else
                    row.AddError(ColumnLabel(Cols.FamilyId), familyRaw, _localizer["import.row.invalidNumber", familyRaw]);
            }

            // Medical conditions (optional, several)
            var conditions = new List<string>();
            var conditionsRaw = Cell(Cols.MedicalConditions);
            if (conditionsRaw is not null)
            {
                foreach (var c in ImportValueParser.SplitList(conditionsRaw))
                {
                    if (c.Length > limits.MedicalCondition)
                    {
                        row.AddError(ColumnLabel(Cols.MedicalConditions), c,
                            _localizer["import.row.tooLong", ColumnLabel(Cols.MedicalConditions), limits.MedicalCondition, c.Length]);
                        break;
                    }
                    conditions.Add(c);
                }
            }

            // Under 18 needs a guardian (same age rule as Trainee.AgeCategory).
            if (birthDate != default && ImportValueParser.AgeOn(birthDate) < 18)
            {
                if (guardian is null)
                    row.AddError(ColumnLabel(Cols.GuardianName), null, _localizer["import.row.guardianRequired", ColumnLabel(Cols.GuardianName)]);
                if (parentNumber is null)
                    row.AddError(ColumnLabel(Cols.ParentNumber), null, _localizer["import.row.guardianRequired", ColumnLabel(Cols.ParentNumber)]);
            }

            // The trainee id is built as {branch}{yy}{MM}{char code of first letter}{counter}
            // (CreateTraineeCommandHandler.CreateTraineeId) and must fit an int. A first name that
            // starts with a non-Latin letter (code point >= 100, e.g. Arabic) at a branch id >= 2
            // overflows it - catch that here with a clear message instead of a failed save.
            if (firstName is not null && branchId > 0 && birthDate != default)
            {
                var first = char.ToUpper(firstName[0]);
                var prefix = $"{branchId}{birthDate.Year % 100:D2}{birthDate.Month:D2}{(int)first:D2}";
                if (prefix.Length + 1 > 10 || long.Parse(prefix + "1", CultureInfo.InvariantCulture) > int.MaxValue)
                    row.AddError(ColumnLabel(Cols.FirstName), firstName, _localizer["import.row.idOverflow", branchId, first.ToString()]);
            }

            if (row.Errors.Count > 0)
                return row;

            var command = new CreateTraineeCommand
            {
                FirstName = firstName!,
                LastName = lastName!,
                SSN = ssn ?? string.Empty,
                PhoneNumber = DigitNormalizer.ToAscii(phoneRaw!),
                Email = email!,
                BirthDate = birthDate,
                Gender = gender,
                Nationality = nationality,
                BranchId = branchId,
                NationalityCategoryId = categoryId,
                SportIds = sportIds,
                FamilyId = row.FamilyId,
                ParentNumber = parentNumber,
                GuardianName = guardian,
                Street = street,
                City = city,
                MedicalConditions = conditions,
            };

            // The exact rules the create-trainee form is held to (names, country-specific
            // phone/national-id formats, email, lengths).
            var validation = await _createValidator.ValidateAsync(command, ct);
            foreach (var failure in validation.Errors)
                row.AddError(ColumnForProperty(failure.PropertyName), failure.AttemptedValue?.ToString(), failure.ErrorMessage);

            if (row.Errors.Count == 0)
                row.Command = command;
            return row;
        }

        private string ColumnForProperty(string property) => ColumnLabel(property switch
        {
            nameof(CreateTraineeCommand.FirstName) => Cols.FirstName,
            nameof(CreateTraineeCommand.LastName) => Cols.LastName,
            nameof(CreateTraineeCommand.SSN) => Cols.Ssn,
            nameof(CreateTraineeCommand.BirthDate) => Cols.BirthDate,
            nameof(CreateTraineeCommand.GuardianName) => Cols.GuardianName,
            nameof(CreateTraineeCommand.ParentNumber) => Cols.ParentNumber,
            nameof(CreateTraineeCommand.PhoneNumber) => Cols.PhoneNumber,
            nameof(CreateTraineeCommand.Email) => Cols.Email,
            nameof(CreateTraineeCommand.Nationality) => Cols.Nationality,
            nameof(CreateTraineeCommand.Gender) => Cols.Gender,
            nameof(CreateTraineeCommand.Street) => Cols.Street,
            nameof(CreateTraineeCommand.City) => Cols.City,
            nameof(CreateTraineeCommand.BranchId) => Cols.Branch,
            nameof(CreateTraineeCommand.NationalityCategoryId) => Cols.NationalityCategory,
            nameof(CreateTraineeCommand.FamilyId) => Cols.FamilyId,
            _ => property,
        });
    }
}
