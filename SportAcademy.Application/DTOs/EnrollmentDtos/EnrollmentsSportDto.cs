using SportAcademy.Application.Common.Pagination;

namespace SportAcademy.Application.DTOs.EnrollmentDtos;

public record EnrollmentsSportDto(
    PagedData<EnrollmentDataDto> Enrollments,
    string SportName
);
