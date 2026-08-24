namespace SportAcademy.Application.DTOs.PaymentTypeDtos
{
    public class PaymentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }
}
