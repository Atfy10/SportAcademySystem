namespace SportAcademy.Application.DTOs.PaymentDtos
{
    public record PaymentHistoryDto(
        string PaymentNumber,
        int PaymentTypeId,
        string PaymentTypeName,
        DateTime PaidDate,
        string BranchName,
        int SubscriptionDetailsId,
        string SubscriptionTypeName,
        string SportName,
        decimal Price,
        DateOnly StartDate,
        DateOnly EndDate,
        // Price is what this payment originally put on the subscription; Refunded is how much
        // of that has since been given back (refund or void). Status is the payment's own.
        decimal Refunded = 0,
        Domain.Enums.PaymentStatus Status = Domain.Enums.PaymentStatus.Completed
    );
}
