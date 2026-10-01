using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Common.CsvImport;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.DTOs.ImportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using System.Globalization;
using Cols = SportAcademy.Application.Common.EmployeeImport.EmployeeImportColumns;

namespace SportAcademy.Application.Services.EmployeeImport
{
    public record EmployeeImportValidationResult(
        EmployeeImportReport Report,
        IReadOnlyList<(int RowNumber, CreateEmployeeCommand Command)> ValidRows);

    // Checks every row of an employee CSV against everything that could make saving it fail -
    // required values, formats, the database's own column lengths, the branch by name or id and
    // the caller's access to it, the create-employee rules (the same FluentValidation validator
    // the Add Employee form goes through), and phone/SSN uniqueness against the database AND
    // within the file - and reports each problem against its row, column and value. Nothing is
    // written; the import only ever saves rows this has passed.
    public class EmployeeImportValidator
    {
        public const int MaxRows = 2000;

        private readonly IEmployeeImportLookup _lookup;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly IValidator<CreateEmployeeCommand> _createValidator;
        private readonly ILocalizationService _localizer;
        private readonly IBranchAccessProvider _branchAccess;

        public EmployeeImportValidator(
            IEmployeeImportLookup lookup,
            IPhoneNumberNormalizer phoneNormalizer,
            IValidator<CreateEmployeeCommand> createValidator,
            ILocalizationService localizer,
            IBranchAccessProvider branchAccess)
        {
            _lookup = lookup;
            _phoneNormalizer = phoneNormalizer;
            _createValidator = createValidator;
            _localizer = localizer;
            _branchAccess = branchAccess;
        }

        // Header text -> canonical column key: canonical names, aliases and the localized column
        // labels in both languages.
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
                    var key = ImportText.Normalize(name);
                    if (key.Length > 0) index.TryAdd(key, col.Key);
                }
            }
            return index;
        }

        public string ColumnLabel(string key) => _localizer[$"import.column.{key}"];

        public async Task<EmployeeImportValidationResult> ValidateAsync(
            IReadOnlyList<ImportRawRow> rawRows, IReadOnlyList<string> headers, CancellationToken ct)
        {
            var fileErrors = new List<string>();
            var headerIndex = BuildHeaderIndex();

            var headerFor = new Dictionary<string, string>();
            var unknown = new List<string>();
            foreach (var header in headers)
            {
                if (string.IsNullOrWhiteSpace(header)) continue;
                if (headerIndex.TryGetValue(ImportText.Normalize(header), out var key))
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
                return new EmployeeImportValidationResult(
                    new EmployeeImportReport(rawRows.Count, 0, rawRows.Count, 0, 0, false, fileErrors, unknown, []), []);

            var branches = await _lookup.GetBranchesAsync(ct);
            var limits = _lookup.GetFieldLimits();
            var genders = ImportValueParser.EnumMatcher<Gender>(_localizer);
            var nationalities = ImportValueParser.EnumMatcher<Nationality>(_localizer);
            var positions = ImportValueParser.EnumMatcher<Position>(_localizer);

            var parsed = new List<ParsedRow>(rawRows.Count);
            foreach (var raw in rawRows)
            {
                ct.ThrowIfCancellationRequested();
                parsed.Add(await ParseRowAsync(raw, headerFor, branches, genders, nationalities, positions, limits, ct));
            }

            // ── Uniqueness against the database ────────────────────────────────────────────
            var existing = await _lookup.GetExistingIdentifiersAsync(
                parsed.Select(p => p.NormalizedPhone ?? ""),
                parsed.Select(p => p.Ssn ?? ""), ct);

            foreach (var p in parsed)
            {
                if (p.NormalizedPhone is { } phone && existing.Phones.Contains(phone))
                    p.AddError(ColumnLabel(Cols.PhoneNumber), p.PhoneRaw, _localizer["employeeImport.row.phoneExists", p.PhoneRaw ?? phone]);
                if (!string.IsNullOrEmpty(p.Ssn) && existing.Ssns.Contains(p.Ssn))
                    p.AddError(ColumnLabel(Cols.Ssn), p.Ssn, _localizer["employeeImport.row.ssnExists"]);
            }

            // ── Uniqueness within the file ─────────────────────────────────────────────────
            FlagInFileDuplicates(parsed, p => p.NormalizedPhone, Cols.PhoneNumber, p => p.PhoneRaw);
            FlagInFileDuplicates(parsed, p => string.IsNullOrEmpty(p.Ssn) ? null : p.Ssn, Cols.Ssn, p => p.Ssn);

            var rows = parsed.Select(p => new EmployeeImportRowReport(
                p.RowNumber,
                p.DisplayName,
                p.Errors.Count == 0 ? ImportRowStatus.Valid : ImportRowStatus.Invalid,
                p.Errors)).ToList();

            var valid = parsed.Where(p => p.Errors.Count == 0 && p.Command is not null)
                .Select(p => (p.RowNumber, p.Command!))
                .ToList();

            var report = new EmployeeImportReport(
                TotalRows: parsed.Count,
                ValidRows: valid.Count,
                InvalidRows: parsed.Count - valid.Count,
                ImportedRows: 0,
                FailedRows: 0,
                Committed: false,
                FileErrors: [],
                UnknownColumns: unknown,
                Rows: rows);

            return new EmployeeImportValidationResult(report, valid);
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
            public CreateEmployeeCommand? Command { get; set; }
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
            Dictionary<string, Gender> genders,
            Dictionary<string, Nationality> nationalities,
            Dictionary<string, Position> positions,
            EmployeeFieldLimits limits,
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

            // Gender / nationality / position
            Gender gender = default;
            var genderRaw = Required(Cols.Gender);
            if (genderRaw is not null && !genders.TryGetValue(ImportText.Normalize(genderRaw), out gender))
                row.AddError(ColumnLabel(Cols.Gender), genderRaw,
                    _localizer["import.row.invalidGender", genderRaw, string.Join(" / ", Enum.GetValues<Gender>().Select(g => _localizer.Label(g)))]);

            Nationality nationality = default;
            var nationalityRaw = Required(Cols.Nationality);
            if (nationalityRaw is not null && !nationalities.TryGetValue(ImportText.Normalize(nationalityRaw), out nationality))
                row.AddError(ColumnLabel(Cols.Nationality), nationalityRaw, _localizer["import.row.invalidNationality", nationalityRaw]);

            Position position = default;
            var positionRaw = Required(Cols.Position);
            if (positionRaw is not null && !positions.TryGetValue(ImportText.Normalize(positionRaw), out position))
                row.AddError(ColumnLabel(Cols.Position), positionRaw,
                    _localizer["employeeImport.row.invalidPosition", positionRaw, string.Join(" / ", Enum.GetValues<Position>().Select(p => _localizer.Label(p)))]);

            // Salary: plain number; thousands separators and a currency code/symbol are tolerated.
            decimal salary = 0;
            var salaryRaw = Required(Cols.Salary);
            if (salaryRaw is not null)
            {
                var text = new string(DigitNormalizer.ToAscii(salaryRaw).Where(c => char.IsDigit(c) || c is '.' or '-').ToArray());
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out salary))
                    row.AddError(ColumnLabel(Cols.Salary), salaryRaw, _localizer["employeeImport.row.invalidSalary", salaryRaw]);
            }

            // Contacts
            var phoneRaw = Limited(Cols.PhoneNumber, Required(Cols.PhoneNumber), 30);
            row.PhoneRaw = phoneRaw;
            if (phoneRaw is not null)
            {
                row.NormalizedPhone = await _phoneNormalizer.NormalizeAsync(DigitNormalizer.ToAscii(phoneRaw), ct);
                if (row.NormalizedPhone is { Length: var len } && len > limits.PhoneNumber)
                    row.AddError(ColumnLabel(Cols.PhoneNumber), phoneRaw, _localizer["import.row.tooLong", ColumnLabel(Cols.PhoneNumber), limits.PhoneNumber, len]);
            }

            // Stored as typed (the handler doesn't normalize it), so it's the typed length that counts.
            var secondRaw = Cell(Cols.SecondPhoneNumber);
            var secondPhone = secondRaw is null ? null : Limited(Cols.SecondPhoneNumber, DigitNormalizer.ToAscii(secondRaw), limits.SecondPhoneNumber);

            var email = Limited(Cols.Email, Required(Cols.Email), limits.Email);

            var ssnRaw = Required(Cols.Ssn);
            var ssn = ssnRaw is null ? null : Limited(Cols.Ssn, DigitNormalizer.ToAscii(ssnRaw).Replace(" ", ""), limits.Ssn);
            row.Ssn = ssn;

            var street = Limited(Cols.Street, Required(Cols.Street), limits.Street);
            var city = Limited(Cols.City, Required(Cols.City), limits.City);

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
                    row.AddError(ColumnLabel(Cols.Branch), branchRaw, _localizer["employeeImport.row.inactiveBranch", branch.Name]);
                else if (_branchAccess.IsRestricted && !_branchAccess.AllowedBranchIds.Contains(branch.Id))
                    row.AddError(ColumnLabel(Cols.Branch), branchRaw, _localizer["import.row.branchNoAccess", branch.Name]);
                else
                    branchId = branch.Id;
            }

            if (row.Errors.Count > 0)
                return row;

            var command = new CreateEmployeeCommand(
                FirstName: firstName!,
                LastName: lastName!,
                SSN: ssn!,
                Salary: salary,
                Gender: gender,
                BirthDate: birthDate,
                Email: email!,
                Nationality: nationality.ToString(),
                Street: street!,
                City: city!,
                PhoneNumber: DigitNormalizer.ToAscii(phoneRaw!),
                SecondNumber: secondPhone,
                Position: position,
                BranchId: branchId,
                CreateUserAccount: false);

            // The exact rules the Add Employee form is held to (names, country-specific phone and
            // national-id formats, email, salary range, minimum age).
            var validation = await _createValidator.ValidateAsync(command, ct);
            foreach (var failure in validation.Errors)
                row.AddError(ColumnForProperty(failure.PropertyName), failure.AttemptedValue?.ToString(), failure.ErrorMessage);

            if (row.Errors.Count == 0)
                row.Command = command;
            return row;
        }

        private string ColumnForProperty(string property) => ColumnLabel(property switch
        {
            nameof(CreateEmployeeCommand.FirstName) => Cols.FirstName,
            nameof(CreateEmployeeCommand.LastName) => Cols.LastName,
            nameof(CreateEmployeeCommand.SSN) => Cols.Ssn,
            nameof(CreateEmployeeCommand.Salary) => Cols.Salary,
            nameof(CreateEmployeeCommand.Gender) => Cols.Gender,
            nameof(CreateEmployeeCommand.BirthDate) => Cols.BirthDate,
            nameof(CreateEmployeeCommand.Email) => Cols.Email,
            nameof(CreateEmployeeCommand.Nationality) => Cols.Nationality,
            nameof(CreateEmployeeCommand.Street) => Cols.Street,
            nameof(CreateEmployeeCommand.City) => Cols.City,
            nameof(CreateEmployeeCommand.PhoneNumber) => Cols.PhoneNumber,
            nameof(CreateEmployeeCommand.SecondNumber) => Cols.SecondPhoneNumber,
            nameof(CreateEmployeeCommand.Position) => Cols.Position,
            nameof(CreateEmployeeCommand.BranchId) => Cols.Branch,
            _ => property,
        });
    }
}
