namespace SportAcademy.Application.Commands.Trainees.ImportTrainees
{
    public class ImportTraineesResult
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportRowError> Errors { get; set; } = [];
    }

    public class ImportRowError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
