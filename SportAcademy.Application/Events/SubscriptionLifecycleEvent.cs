using MediatR;

namespace SportAcademy.Application.Events;

/// Covers Suspend/Activate/Delete on a SubscriptionDetails - one shared event since all three
/// are "subscription lifecycle changed", differing only by which action happened.
public sealed record SubscriptionLifecycleEvent(int SubscriptionId, string Action, string ActorName) : INotification;
