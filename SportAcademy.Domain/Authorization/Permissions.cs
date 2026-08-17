namespace SportAcademy.Domain.Authorization
{
    // Permission strings stored as ASP.NET Identity role claims (ClaimType = "permission").
    // Each one names a real business action, not a CRUD abstraction - a role either grants a
    // capability or it doesn't, independent of what the seeded role's name implies. This is
    // additive to (not a replacement for) the existing [Authorize(Roles=...)] checks that
    // gate SuperAdmin/platform-only surfaces.
    public static class Permissions
    {
        public static class Trainee
        {
            public const string Register = "trainee.register";
            public const string Edit = "trainee.edit";
            public const string Delete = "trainee.delete";
            public const string Export = "trainee.export";
        }

        public static class Enrollment
        {
            public const string Create = "enrollment.create";
            public const string Edit = "enrollment.edit";
            public const string Activate = "enrollment.activate";
        }

        public static class Attendance
        {
            public const string Mark = "attendance.mark";
            public const string ViewRate = "attendance.view_rate";
        }

        public static class TraineeGroup
        {
            public const string Manage = "traineegroup.manage";
            public const string GenerateSessions = "traineegroup.generate_sessions";
        }

        public static class Employee
        {
            public const string Manage = "employee.manage";
        }

        public static class Coach
        {
            public const string Manage = "coach.manage";
        }

        public static class Branch
        {
            public const string Manage = "branch.manage";
        }

        public static class Sport
        {
            public const string Manage = "sport.manage";
        }

        public static class Payment
        {
            public const string Record = "payment.record";
            public const string Correct = "payment.correct";
        }

        public static class Tenant
        {
            public const string ManageSettings = "tenant.settings.manage";
            public const string ManageUsers = "tenant.users.manage";
        }

        public static class Platform
        {
            public const string ManageTenants = "platform.tenants.manage";
        }

        // All permissions in the catalog, used to validate seed data / diagnostics without
        // hand-maintaining a second list.
        public static readonly IReadOnlyList<string> All =
        [
            Trainee.Register, Trainee.Edit, Trainee.Delete, Trainee.Export,
            Enrollment.Create, Enrollment.Edit, Enrollment.Activate,
            Attendance.Mark, Attendance.ViewRate,
            TraineeGroup.Manage, TraineeGroup.GenerateSessions,
            Employee.Manage,
            Coach.Manage,
            Branch.Manage,
            Sport.Manage,
            Payment.Record, Payment.Correct,
            Tenant.ManageSettings, Tenant.ManageUsers,
            Platform.ManageTenants,
        ];
    }
}
