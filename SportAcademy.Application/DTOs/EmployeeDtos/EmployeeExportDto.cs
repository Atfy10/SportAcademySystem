namespace SportAcademy.Application.DTOs.EmployeeDtos;

// One employee as the console's CSV export writes it. The first block is exactly what the
// importer reads (enum NAMES and the branch NAME, never ids), so an exported file can be edited
// and imported back; the rest are read-only extras the importer ignores.
public class EmployeeExportDto
{
    // ── Importable ───────────────────────────────────────────────────────
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SSN { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? SecondPhoneNumber { get; set; }

    // ── Read-only extras ─────────────────────────────────────────────────
    public int Id { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsWorking { get; set; }
    public bool HasLoginAccount { get; set; }
    public bool IsCoach { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
