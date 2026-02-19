namespace SportAcademy.Application.DTOs.TraineeGroupDtos
{
    public record ListTraineeGroupDto(
        int Id,
        string SportName,
        string CoachName,
        string BranchName,
        int DurationInMinutes,
        int TraineesCount,
        TimeOnly StartTime
    );
}
