using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class PaymentTypeMappings
    {
        public static PaymentTypeDto ToDto(this PaymentType entity)
        {
            return new PaymentTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault,
            };
        }
    }
}
