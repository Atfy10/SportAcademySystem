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
        DateOnly EndDate
    );
}
