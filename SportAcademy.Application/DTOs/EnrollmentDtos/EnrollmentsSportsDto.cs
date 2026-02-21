namespace SportAcademy.Application.DTOs.EnrollmentDtos;

public record EnrollmentsSportsDto(
    IReadOnlyList<EnrollmentDataDto> Enrollments,
    string SportName
);
