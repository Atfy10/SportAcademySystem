namespace SportAcademy.Application.Common.Pagination
{
    public interface IPaginatedRequest
    {
        PageRequest Page { get; set; }
    }
}
