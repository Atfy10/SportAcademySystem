using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Helpers;

/// <summary>
/// Compile-time catalog of every business event key that can go through the notification
/// channel pipeline - one const per INotificationService caller in Application/EventHandlers.
/// Mirrors NotificationGroupNames' role as the single source of truth event handlers reference
/// directly, instead of duplicating raw strings. Seeded into NotificationEventType by
/// AppDataSeeder.SeedNotificationEventTypesAsync; a key used here with no matching seeded row is
/// simply not selectable in the routing matrix yet, not an error.
/// </summary>
public static class NotificationEventTypes
{
    public const string EmployeeLifecycle = "employee-lifecycle";
    public const string EmployeeStatusChanged = "employee-status-changed";
    public const string EnrollmentLifecycle = "enrollment-lifecycle";
    public const string ExcuseRequestCreated = "excuse-request-created";
    public const string ExcuseRequestReviewed = "excuse-request-reviewed";
    public const string FirstTenantDashboardLoad = "first-tenant-dashboard-load";
    public const string ImpersonationStarted = "impersonation-started";
    public const string InvitationAccepted = "invitation-accepted";
    public const string InvitationCreated = "invitation-created";
    public const string PasswordResetByAdmin = "password-reset-by-admin";
    public const string PaymentRecorded = "payment-recorded";
    public const string PaymentRefunded = "payment-refunded";
    public const string PaymentVoided = "payment-voided";
    public const string SalaryPaymentCreated = "salary-payment-created";
    public const string SubscriptionCreated = "subscription-created";
    public const string SubscriptionDiscountRequestCreated = "subscription-discount-request-created";
    public const string SubscriptionDiscountRequestReviewed = "subscription-discount-request-reviewed";
    public const string SubscriptionLifecycle = "subscription-lifecycle";
    public const string TraineeGroupCreated = "trainee-group-created";
    public const string TraineeGroupDeleted = "trainee-group-deleted";
    public const string UserActiveStatusChanged = "user-active-status-changed";
    public const string UserBranchesChanged = "user-branches-changed";
    public const string UserRolesChanged = "user-roles-changed";

    /// <summary>Catalog rows seeded by AppDataSeeder - (Key, DisplayName, Description, DefaultStyle).
    /// One entry per const above, in the same order.</summary>
    public static readonly (string Key, string DisplayName, string Description, NotificationType DefaultStyle)[] Catalog =
    [
        (EmployeeLifecycle, "Employee Lifecycle", "An employee record was created, updated, or removed", NotificationType.System),
        (EmployeeStatusChanged, "Employee Status Changed", "An employee was activated or deactivated", NotificationType.System),
        (EnrollmentLifecycle, "Enrollment Lifecycle", "A trainee enrollment was created, suspended, reactivated, or ended", NotificationType.System),
        (ExcuseRequestCreated, "Excuse Request Submitted", "A trainee's excused-absence request was submitted for review", NotificationType.Warning),
        (ExcuseRequestReviewed, "Excuse Request Reviewed", "An excused-absence request was approved or rejected", NotificationType.System),
        (FirstTenantDashboardLoad, "First Dashboard Load", "A tenant's dashboard was opened for the first time", NotificationType.System),
        (ImpersonationStarted, "Impersonation Started", "A SuperAdmin started impersonating this tenant", NotificationType.Warning),
        (InvitationAccepted, "Invitation Accepted", "An invited user accepted their invitation and joined", NotificationType.System),
        (InvitationCreated, "Invitation Sent", "A new user invitation was created", NotificationType.System),
        (PasswordResetByAdmin, "Password Reset by Admin", "An admin reset a user's password on their behalf", NotificationType.Warning),
        (PaymentRecorded, "Payment Recorded", "A payment was recorded", NotificationType.System),
        (PaymentRefunded, "Payment Refunded", "A payment was refunded", NotificationType.Warning),
        (PaymentVoided, "Payment Voided", "A payment was voided", NotificationType.Warning),
        (SalaryPaymentCreated, "Salary Payment Recorded", "A coach/employee salary payment was recorded", NotificationType.System),
        (SubscriptionCreated, "Subscription Created", "A new subscription was created", NotificationType.System),
        (SubscriptionDiscountRequestCreated, "Discount Request Submitted", "A subscription discount request was submitted for review", NotificationType.Warning),
        (SubscriptionDiscountRequestReviewed, "Discount Request Reviewed", "A subscription discount request was approved or rejected", NotificationType.System),
        (SubscriptionLifecycle, "Subscription Lifecycle", "A subscription was suspended, activated, or otherwise changed", NotificationType.Warning),
        (TraineeGroupCreated, "Trainee Group Created", "A new trainee group was created", NotificationType.System),
        (TraineeGroupDeleted, "Trainee Group Deleted", "A trainee group was deleted", NotificationType.Warning),
        (UserActiveStatusChanged, "User Status Changed", "A user account was activated or deactivated", NotificationType.Warning),
        (UserBranchesChanged, "User Branches Changed", "A user's assigned branches changed", NotificationType.System),
        (UserRolesChanged, "User Roles Changed", "A user's assigned roles changed", NotificationType.Warning),
    ];

    /// <summary>
    /// Phase-1 default for the Email channel: on only for these personally-actionable,
    /// single-recipient events (the affected person learning something happened to their own
    /// account/request) - every broadcast-style Admin/Owner event defaults off, or every
    /// payment-recorded event would suddenly email every Owner. Referenced by both
    /// AppDataSeeder (to seed the actual visible TenantNotificationChannelRule rows) and
    /// NotificationChannelDispatcher (as the defensive fallback for a tenant/event combination
    /// that hasn't been reconciled yet) - single source of truth so the two can never drift.
    /// </summary>
    public static readonly HashSet<string> DefaultEmailOnKeys =
    [
        ImpersonationStarted,
        InvitationAccepted,
        PasswordResetByAdmin,
        UserActiveStatusChanged,
        UserBranchesChanged,
        UserRolesChanged,
        ExcuseRequestReviewed,
        SubscriptionDiscountRequestReviewed,
    ];
}
