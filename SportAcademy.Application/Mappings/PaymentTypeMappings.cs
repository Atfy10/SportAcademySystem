using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class PaymentTypeMappings
    {
        /// <summary>
        /// Requires entity.Translations to already be loaded (see PaymentTypeRepository, which
        /// Includes it) - this is pure in-memory selection over whatever the caller fetched, not
        /// a query, so there is nothing to translate to SQL here.
        /// </summary>
        public static PaymentTypeDto ToDto(this PaymentType entity, string lang)
        {
            return new PaymentTypeDto
            {
                Id = entity.Id,
                Name = entity.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                       ?? entity.Name,
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault,
            };
        }
    }
}
