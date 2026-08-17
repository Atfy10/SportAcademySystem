namespace SportAcademy.Application.Interfaces
{
    // Implemented by commands/queries whose tenant feature must be enabled to execute.
    // FeatureKey matches the Feature.Name seeded on the backend and the frontend's
    // FEATURE_DEFS key (e.g. "attendance-tracking") - the same string used for nav/route
    // gating, so what's hidden on the frontend and what's enforced on the backend stay in sync.
    public interface IRequiresFeature
    {
        string FeatureKey { get; }
    }
}
