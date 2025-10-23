using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SessionOccurrenceDtos
{
    public record SessionOccurrenceDto(
        int Id,
        int GroupScheduleId,
        DateTime StartDateTime,
        SessionStatus Status
    );
}
