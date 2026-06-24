using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities;

public class NotificationGroupMember
{
    public Guid UserId { get; set; }
    public string GroupName { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
