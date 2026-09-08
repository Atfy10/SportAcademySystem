using FluentAssertions;
using SportAcademy.Application.Common.Features;

namespace SportAcademy.Tests.Application.Common;

// Pure logic tests for FeatureDependencyPolicy against the real FeatureDependencies edge map
// (attendance-tracking -> session-management/enrollment-management, coach-management ->
// branch-management, etc.) - no repository/mocking needed since the policy only takes plain
// name-keyed data in and returns plain data out.
public class FeatureDependencyPolicyTests
{
    [Fact]
    public void ValidateEnable_MissingPrerequisite_ReturnsBlockingMessage()
    {
        var error = FeatureDependencyPolicy.ValidateEnable("coach-management", new HashSet<string>());

        error.Should().NotBeNull();
        error.Should().Contain("branch-management");
    }

    [Fact]
    public void ValidateEnable_PrerequisiteAlreadyEnabled_ReturnsNull()
    {
        var currentlyEnabled = new HashSet<string> { "branch-management" };

        var error = FeatureDependencyPolicy.ValidateEnable("coach-management", currentlyEnabled);

        // coach-management only requires branch-management, which is already enabled.
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateEnable_FeatureWithNoPrerequisites_ReturnsNull()
    {
        var error = FeatureDependencyPolicy.ValidateEnable("notifications", new HashSet<string>());

        error.Should().BeNull();
    }

    [Fact]
    public void ValidateDisable_DependentStillEnabled_ReturnsBlockingMessage()
    {
        var currentlyEnabled = new HashSet<string> { "coach-management" };

        var error = FeatureDependencyPolicy.ValidateDisable("branch-management", currentlyEnabled);

        error.Should().NotBeNull();
        error.Should().Contain("coach-management");
    }

    [Fact]
    public void ValidateDisable_NoDependentsEnabled_ReturnsNull()
    {
        var error = FeatureDependencyPolicy.ValidateDisable("branch-management", new HashSet<string>());

        error.Should().BeNull();
    }

    [Fact]
    public void ValidateEndState_EnablesDependentWithoutPrerequisiteInSameBatch_ReportsError()
    {
        var endState = new Dictionary<string, bool>
        {
            ["attendance-tracking"] = true,
            ["session-management"] = false,
            ["enrollment-management"] = true,
        };

        var errors = FeatureDependencyPolicy.ValidateEndState(endState);

        errors.Should().ContainSingle(e => e.Contains("attendance-tracking") && e.Contains("session-management"));
    }

    [Fact]
    public void ValidateEndState_EnablesDependentAndPrerequisiteTogether_NoErrors()
    {
        var endState = new Dictionary<string, bool>
        {
            ["attendance-tracking"] = true,
            ["session-management"] = true,
            ["enrollment-management"] = true,
            ["group-management"] = true,
            ["schedule-management"] = true,
            ["trainee-management"] = true,
            ["subscription-plan"] = true,
            ["branch-management"] = true,
            ["sport-management"] = true,
        };

        var errors = FeatureDependencyPolicy.ValidateEndState(endState);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void ResolveCascade_EnablingDependent_IncludesTransitivePrerequisites()
    {
        var state = new Dictionary<string, FeatureRuntimeState>
        {
            ["attendance-tracking"] = new(false, false),
            ["session-management"] = new(false, false),
            ["enrollment-management"] = new(false, false),
            ["group-management"] = new(false, false),
            ["schedule-management"] = new(false, false),
            ["trainee-management"] = new(false, false),
            ["subscription-plan"] = new(false, false),
            ["branch-management"] = new(false, false),
            ["sport-management"] = new(false, false),
        };

        var result = FeatureDependencyPolicy.ResolveCascade("attendance-tracking", true, state);

        result.IsBlocked.Should().BeFalse();
        result.AffectedFeatureNames.Should().Contain([
            "session-management", "enrollment-management", "group-management",
            "schedule-management", "trainee-management", "subscription-plan",
            "branch-management", "sport-management",
        ]);
    }

    [Fact]
    public void ResolveCascade_DisablingPrerequisite_IncludesTransitiveDependents()
    {
        var state = new Dictionary<string, FeatureRuntimeState>
        {
            ["branch-management"] = new(true, false),
            ["coach-management"] = new(true, false),
            ["group-management"] = new(true, false),
            ["session-management"] = new(true, false),
            ["attendance-tracking"] = new(true, false),
        };

        var result = FeatureDependencyPolicy.ResolveCascade("branch-management", false, state);

        result.IsBlocked.Should().BeFalse();
        result.AffectedFeatureNames.Should().Contain([
            "coach-management", "group-management", "session-management", "attendance-tracking",
        ]);
    }

    [Fact]
    public void ResolveCascade_DisablingPrerequisite_LockedDependentBlocksTheWholeOperation()
    {
        var state = new Dictionary<string, FeatureRuntimeState>
        {
            ["branch-management"] = new(true, false),
            ["coach-management"] = new(true, true), // locked by a prior SuperAdmin decision
        };

        var result = FeatureDependencyPolicy.ResolveCascade("branch-management", false, state);

        result.IsBlocked.Should().BeTrue();
        result.LockedConflicts.Should().Contain("coach-management");
    }

    [Fact]
    public void ResolveCascade_EnablingPrerequisite_LockedOffPrerequisiteBlocksTheWholeOperation()
    {
        // coach-management requires branch-management. If branch-management was explicitly
        // locked OFF by a SuperAdmin, cascading it back on would silently override that decision.
        var state = new Dictionary<string, FeatureRuntimeState>
        {
            ["coach-management"] = new(false, false),
            ["branch-management"] = new(false, true),
        };

        var result = FeatureDependencyPolicy.ResolveCascade("coach-management", true, state);

        result.IsBlocked.Should().BeTrue();
        result.LockedConflicts.Should().Contain("branch-management");
    }

    [Fact]
    public void ValidateProtectedDisable_ProtectedFeature_ReturnsBlockingMessage()
    {
        var error = FeatureDependencyPolicy.ValidateProtectedDisable("user-management");

        error.Should().NotBeNull();
        error.Should().Contain("user-management");
    }

    [Fact]
    public void ValidateProtectedDisable_UnprotectedFeature_ReturnsNull()
    {
        var error = FeatureDependencyPolicy.ValidateProtectedDisable("coach-management");

        error.Should().BeNull();
    }

    [Fact]
    public void ValidateDisable_ProtectedFeature_BlockedEvenWithNoDependents()
    {
        var error = FeatureDependencyPolicy.ValidateDisable("tenant-settings", new HashSet<string>());

        error.Should().NotBeNull();
    }

    [Fact]
    public void ValidateEndState_DisablingAProtectedFeature_ReportsError()
    {
        var endState = new Dictionary<string, bool> { ["role-management"] = false };

        var errors = FeatureDependencyPolicy.ValidateEndState(endState);

        errors.Should().ContainSingle(e => e.Contains("role-management"));
    }

    [Fact]
    public void ValidatePlanFeatureSetClosed_MissingPrerequisite_ReportsError()
    {
        var planFeatures = new HashSet<string> { "attendance-tracking", "enrollment-management" };

        var errors = FeatureDependencyPolicy.ValidatePlanFeatureSetClosed(planFeatures);

        errors.Should().ContainSingle(e => e.Contains("attendance-tracking") && e.Contains("session-management"));
    }

    [Fact]
    public void ValidatePlanFeatureSetClosed_ClosedSet_NoErrors()
    {
        var planFeatures = new HashSet<string>
        {
            "attendance-tracking", "session-management", "enrollment-management",
            "group-management", "schedule-management", "trainee-management",
            "subscription-plan", "branch-management", "sport-management",
        };

        var errors = FeatureDependencyPolicy.ValidatePlanFeatureSetClosed(planFeatures);

        errors.Should().BeEmpty();
    }
}
