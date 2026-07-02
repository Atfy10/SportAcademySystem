using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Application.Mappings;

public static class TenantMapper
{
    public static CurrentTenantResponse ToCurrentResponse(this Tenant entity)
    {
        return new CurrentTenantResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Slug = entity.Slug,
            Status = entity.Status.ToString()
        };
    }
}
