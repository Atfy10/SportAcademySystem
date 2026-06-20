using SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class SessionOccurrenceMappings
    {
        public static SessionOccurrence ToSessionOccurrence(this CreateSessionOccurrenceCommand cmd)
        {
            return SessionOccurrence.Create(
                cmd.GroupScheduleId,
                cmd.StartDateTime,
                cmd.Status);
        }

        public static SessionOccurrenceDto ToDto(this SessionOccurrence so)
        {
            return new SessionOccurrenceDto(
                so.Id,
                so.GroupSchedule!.TraineeGroup.Id,
                DateOnly.FromDateTime(so.StartDateTime),
                so.GroupSchedule!.TraineeGroup!.Coach!.Sport!.Name,
                $"{so.GroupSchedule!.TraineeGroup!.Coach.Employee!.FirstName} {so.GroupSchedule!.TraineeGroup!.Coach.Employee.LastName}",
                so.GroupSchedule!.TraineeGroup!.Branch!.Name,
                so.StartDateTime.ToString("HH:mm:ss"),
                so.GroupSchedule!.TraineeGroup!.DurationInMinutes,
                so.GroupSchedule!.TraineeGroup!.Enrollments.Count(e => e.IsActive),
                0,
                0,
                0
            );
        }
    }
}
