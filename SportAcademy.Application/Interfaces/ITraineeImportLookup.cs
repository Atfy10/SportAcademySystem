namespace SportAcademy.Application.Interfaces
{
    // A named reference row the CSV may point at by name (in any language) or by id.
    public record ImportNamedItem(int Id, string Name, IReadOnlyList<string> AlternateNames, bool IsActive);

    // The database's own column limits for the trainee fields an import writes - read from the
    // EF model, not duplicated here, so pre-validation can never drift from what the database
    // will actually accept.
    public record TraineeFieldLimits(
        int FirstName, int LastName, int Ssn, int PhoneNumber, int ParentNumber,
        int GuardianName, int Email, int Street, int City, int MedicalCondition);

    // What already exists, for the "already registered" checks - one query per kind for the
    // whole file instead of three per row.
    public record ExistingTraineeIdentifiers(
        IReadOnlySet<string> Phones, IReadOnlySet<string> Ssns, IReadOnlySet<string> Emails);

    // Everything the trainee CSV pre-validation needs from the database, in a few set-based
    // queries. Tenant-scoped by the usual query filters.
    public interface ITraineeImportLookup
    {
        Task<IReadOnlyList<ImportNamedItem>> GetBranchesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ImportNamedItem>> GetNationalityCategoriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ImportNamedItem>> GetSportsAsync(CancellationToken ct = default);
        Task<IReadOnlySet<int>> GetExistingFamilyIdsAsync(IEnumerable<int> familyIds, CancellationToken ct = default);
        Task<ExistingTraineeIdentifiers> GetExistingIdentifiersAsync(
            IEnumerable<string> phones, IEnumerable<string> ssns, IEnumerable<string> emails, CancellationToken ct = default);
        TraineeFieldLimits GetFieldLimits();
    }
}
