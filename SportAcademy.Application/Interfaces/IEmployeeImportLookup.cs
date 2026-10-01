namespace SportAcademy.Application.Interfaces
{
    // The database's own column limits for the employee fields an import writes - read from the
    // EF model so pre-validation can never drift from what the database will accept.
    public record EmployeeFieldLimits(
        int FirstName, int LastName, int Ssn, int PhoneNumber, int SecondPhoneNumber,
        int Email, int Street, int City);

    // What already exists, for the "already registered" checks CreateEmployeeCommandHandler
    // rejects on (phone, SSN) - one query per kind for the whole file.
    public record ExistingEmployeeIdentifiers(IReadOnlySet<string> Phones, IReadOnlySet<string> Ssns);

    // Everything the employee CSV pre-validation needs from the database. Tenant-scoped by the
    // usual query filters.
    public interface IEmployeeImportLookup
    {
        Task<IReadOnlyList<ImportNamedItem>> GetBranchesAsync(CancellationToken ct = default);
        Task<ExistingEmployeeIdentifiers> GetExistingIdentifiersAsync(
            IEnumerable<string> phones, IEnumerable<string> ssns, CancellationToken ct = default);
        EmployeeFieldLimits GetFieldLimits();
    }
}
