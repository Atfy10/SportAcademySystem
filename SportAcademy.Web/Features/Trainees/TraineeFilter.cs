namespace SportAcademy.Web.Features.Trainees;

public class TraineeFilter
{
    // Was a sport-name string matched against Sport.Name in raw SQL - that stopped working the
    // moment a sport's displayed name could differ from its stored name (Arabic translations).
    // The frontend now sends the id; the dropdown already had it.
    public int? SportId { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}
