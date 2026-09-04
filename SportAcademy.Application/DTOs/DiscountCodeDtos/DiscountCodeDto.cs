namespace SportAcademy.Application.DTOs.DiscountCodeDtos;

public record DiscountCodeDto(
    int Id,
    string Code,
    string? Description,
    decimal PercentageOff,
    bool IsActive,
    DateOnly? ExpiresAt);

// Returned by the validate-preview lookup for a currently valid/active/unexpired code - no
// Description/IsActive/ExpiresAt, since the caller (SubscriptionFormModal) only needs the
// percentage to compute the discounted price to display.
public record DiscountCodeValidationDto(int Id, string Code, decimal PercentageOff);
