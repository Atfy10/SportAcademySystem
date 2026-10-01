using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.EventDtos;

// Every *Local time here is the academy's wall clock (its configured time zone, no offset).

// One ticket as the event page lists it. Token is what the ticket's QR code links to
// (/ticket/{Token}) - staff share it with the guest.
public record EventTicketDto(
    int Id,
    int Number,
    string Token,
    string? GuestName,
    DateTime IssuedAt,
    DateTime? AdmittedAtLocal,
    string? AdmittedByName);

// The event page's tickets card. Terminated: the event has ended or was cancelled, so every
// ticket is closed for good (read-only record of who came).
public record EventTicketsDto(
    int EventId,
    int Capacity,
    int Issued,
    int Admitted,
    bool Terminated,
    DateTime EntryOpensAtLocal,
    PagedData<EventTicketDto> Tickets);

// What the door scanner shows after a scan (or an admit). For Invalid nothing else is filled in.
// AdmittedAtLocal/AdmittedByName are set for AlreadyUsed (and Admitted).
public record EventTicketCheckDto(
    EventTicketCheckResult Result,
    int? TicketId = null,
    int? Number = null,
    string? GuestName = null,
    int? EventId = null,
    string? EventTitle = null,
    string? BranchName = null,
    DateTime? StartsAtLocal = null,
    DateTime? EndsAtLocal = null,
    DateTime? OpensAtLocal = null,
    int? Capacity = null,
    int? AdmittedCount = null,
    DateTime? AdmittedAtLocal = null,
    string? AdmittedByName = null);

// What the guest's own ticket page (/ticket/{token}, no login) shows. Valid / NotYetOpen: show
// the code. AlreadyUsed: the ticket has been used (UsedAtLocal), no code. Ended / Cancelled: the
// ticket is terminated - only the academy and event names, no code and no ticket details.
// Invalid: nothing at all.
public record PublicEventTicketDto(
    EventTicketCheckResult Result,
    string? AcademyName = null,
    string? EventTitle = null,
    string? BranchName = null,
    DateTime? StartsAtLocal = null,
    DateTime? EndsAtLocal = null,
    DateTime? OpensAtLocal = null,
    int? Number = null,
    string? GuestName = null,
    DateTime? UsedAtLocal = null);

// An event the door scanner can let people in to today (not ended, not cancelled). IsOpen: the
// entry window is open right now.
public record CheckInEventDto(
    int Id,
    string Title,
    string BranchName,
    DateTime StartsAtLocal,
    DateTime EndsAtLocal,
    DateTime OpensAtLocal,
    int Capacity,
    int Issued,
    int Admitted,
    bool IsOpen);
