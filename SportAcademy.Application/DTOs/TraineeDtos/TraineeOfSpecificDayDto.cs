using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeOfSpecificDayDto(
    int Id,
    string FullName,
    int Age,
    string Email,
    string PhoneNumber,
    DateOnly? JoinDate,
    bool IsSubscribed,
    int AttendanceRate,
    TraineeSportDto TraineeSports
);