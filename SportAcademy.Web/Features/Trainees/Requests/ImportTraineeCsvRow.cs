using CsvHelper.Configuration;
using System.Globalization;

namespace SportAcademy.Web.Features.Trainees.Requests;

public class ImportTraineeCsvRow
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SSN { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string NationalityCategoryId { get; set; } = string.Empty;
    public string SportIds { get; set; } = string.Empty;
    public string? FamilyId { get; set; }
    public string? ParentNumber { get; set; }
    public string? GuardianName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
}

public sealed class ImportTraineeCsvRowMap : ClassMap<ImportTraineeCsvRow>
{
    public ImportTraineeCsvRowMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.SportIds).Name("sportIds");
    }
}
