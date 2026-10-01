using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.EventDtos;

// What the public entry page shows after a scan. For Invalid, nothing else is filled in - an
// unknown code learns nothing about any event. Times are the academy's wall clock.
public record EventEntryResultDto(
    EventEntryResult Result,
    string? EventTitle = null,
    string? AcademyName = null,
    string? BranchName = null,
    DateTime? StartsAtLocal = null,
    DateTime? EndsAtLocal = null,
    DateTime? OpensAtLocal = null,
    int? Capacity = null,
    int? AdmittedCount = null,
    int? AdmissionNumber = null,
    DateTime? AdmittedAtLocal = null);
