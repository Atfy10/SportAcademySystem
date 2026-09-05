using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;

public record SubscriptionDiscountRequestDto(
    int Id,
    int TraineeId,
    string TraineeName,
    int SubscriptionTypeId,
    string SubscriptionTypeName,
    int SportId,
    string SportName,
    int BranchId,
    string BranchName,
    DateOnly StartDate,
    TraineeGroupType GroupType,
    List<DayOfWeek> TrainingDays,
    int PaymentTypeId,
    string DiscountCode,
    SubscriptionDiscountRequestStatus Status,
    Guid RequestedByUserId,
    string RequestedByName,
    DateTime RequestedAt,
    Guid? ReviewedByUserId,
    string? ReviewedByName,
    DateTime? ReviewedAt,
    string? RejectionReason,
    int? CreatedSubscriptionDetailsId);
