using MediatR;

namespace SportAcademy.Application.Events;

public sealed record PasswordResetByAdminEvent(Guid TargetUserId, string ActorName) : INotification;
