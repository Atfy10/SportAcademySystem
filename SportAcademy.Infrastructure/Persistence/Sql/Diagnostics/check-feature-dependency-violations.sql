-- Read-only diagnostic. Run before the feature-dependency policy (FeatureDependencyPolicy.cs /
-- FeatureDependencies.cs) goes live against a database with existing tenant data.
--
-- Reports every tenant that currently has a feature enabled without one of its prerequisites
-- also enabled - a state the new self-service/cascade rules will refuse to create going
-- forward, but that may already exist from before this policy existed (e.g. a tenant enabled
-- "attendance-tracking" back when nothing checked that "session-management" was also on).
--
-- This script does not modify anything. Existing inconsistent tenants are NOT auto-remediated -
-- see FeatureDependencyPolicy.cs's header comment for why. A SuperAdmin resolves each one
-- manually via the Platform console (which cascades) once this report is reviewed.
--
-- IMPORTANT: the edge list below (@Edges) must be kept in sync by hand with
-- SportAcademy.Application/Common/Features/FeatureDependencies.cs - there is deliberately no
-- single source shared between application code and this ops script.

DECLARE @Edges TABLE (FeatureName nvarchar(100), RequiresFeatureName nvarchar(100));
INSERT INTO @Edges (FeatureName, RequiresFeatureName) VALUES
    (N'attendance-tracking', N'session-management'),
    (N'attendance-tracking', N'enrollment-management'),
    (N'session-management', N'group-management'),
    (N'session-management', N'schedule-management'),
    (N'enrollment-management', N'trainee-management'),
    (N'enrollment-management', N'subscription-plan'),
    (N'group-management', N'branch-management'),
    (N'group-management', N'sport-management'),
    (N'coach-management', N'branch-management'),
    (N'pricing-management', N'sport-management'),
    (N'pricing-management', N'branch-management'),
    (N'discount-offers', N'subscription-plan'),
    (N'family-management', N'trainee-management');

SELECT
    t.Id AS TenantId,
    t.Name AS TenantName,
    t.Slug AS TenantSlug,
    f.Name AS EnabledFeature,
    e.RequiresFeatureName AS MissingPrerequisite
FROM @Edges e
JOIN Features f ON f.Name = e.FeatureName
JOIN TenantFeatures tf ON tf.FeatureId = f.Id AND tf.IsEnabled = 1
JOIN Tenants t ON t.Id = tf.TenantId
JOIN Features pf ON pf.Name = e.RequiresFeatureName
WHERE NOT EXISTS (
    SELECT 1
    FROM TenantFeatures ptf
    WHERE ptf.TenantId = tf.TenantId
      AND ptf.FeatureId = pf.Id
      AND ptf.IsEnabled = 1
)
ORDER BY t.Name, f.Name, e.RequiresFeatureName;
