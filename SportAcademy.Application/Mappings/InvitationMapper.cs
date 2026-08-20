using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings;

public static class InvitationMapper
{
    public static Invitation ToEntity(
        Guid tenantId, string email, Guid invitedByUserId, string tokenHash, DateTime expiresAt,
        string? role = null, IReadOnlyList<string>? permissions = null)
    {
        return new Invitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            TokenHash = tokenHash,
            Purpose = role is null ? InvitationPurpose.OwnerSetup : InvitationPurpose.StaffOnboarding,
            Status = InvitationStatus.Pending,
            ExpiresAt = expiresAt,
            InvitedByUserId = invitedByUserId,
            Role = role,
            Permissions = permissions is { Count: > 0 } ? string.Join(",", permissions) : null
        };
    }

    public static InvitationResponse ToResponse(this Invitation entity)
    {
        return new InvitationResponse
        {
            Id = entity.Id,
            Email = entity.Email,
            Status = entity.Status.ToString(),
            IsExpired = entity.ExpiresAt < DateTime.UtcNow,
            ExpiresAt = entity.ExpiresAt,
            CreatedAt = entity.CreatedAt
        };
    }
}
