namespace SportAcademy.Application.Interfaces;

// Implemented by commands that create or reactivate a limited resource. Opt-in per command,
// exactly like IRequiresFeature - LimitGateBehavior only gates a request that implements this,
// not a blanket check on every request. ResourceKey is one of LimitedResources' constants.
public interface IConsumesLimit
{
    string ResourceKey { get; }
}
