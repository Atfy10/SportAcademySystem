using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings
{
    public static class PaymentMappings
    {
        public static Payment ToPayment(this string paymentNumber, PaymentMethod method, int branchId)
        {
            return Payment.Create(paymentNumber, method, branchId);
        }
    }
}
