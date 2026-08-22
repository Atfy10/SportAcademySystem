using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // Collapses the seven seeded roles (SuperAdmin, Owner, Admin, Manager, User, Coach,
    // Accountant) down to the four the business actually has (plus the platform-only
    // SuperAdmin): Manager -> Admin, Coach -> Employee (new role), User -> Employee. Also
    // reconciles AspNetRoleClaims for the five surviving roles to the current
    // AppDataSeeder.DefaultRolePermissions, since the seeder itself is a no-op on any database
    // that already has a tenant and so cannot fix an existing deployment's role claims on its
    // own (see AppDataSeeder.SeedAsync's early-return guard).
    //
    // IRREVERSIBLE: Down() recreates the three retired roles but does not attempt to restore
    // which users held them - that mapping is destroyed by this migration's Up() by design.
    // Take a database backup before applying this migration to any environment with real data.
    public partial class ConsolidateRolesToFour : Migration
    {
        // Every permission in SportAcademy.Domain.Authorization.Permissions.All. Kept as a
        // literal here (rather than referencing the C# constants) because a migration's Up()
        // must remain valid forever, independent of future edits to that catalog - but it must
        // be kept in sync with it by hand whenever the catalog changes. As of this migration:
        private const string AllPermissionsCsv =
            "trainee.register,trainee.edit,trainee.delete,trainee.export," +
            "enrollment.create,enrollment.edit,enrollment.activate," +
            "attendance.mark,attendance.view_rate," +
            "traineegroup.manage,traineegroup.generate_sessions," +
            "subscription.manage,subscriptiontype.manage,session.manage,profile.manage," +
            "employee.manage,coach.manage,branch.manage,sport.manage," +
            "payment.record,payment.correct,payment.refund,payment.view," +
            "finance.view," +
            "report.view,report.export," +
            "tenant.settings.manage,tenant.users.manage," +
            "platform.tenants.manage";

        private const string EmployeePermissionsCsv =
            "trainee.register,trainee.edit,trainee.export," +
            "enrollment.create,enrollment.edit,enrollment.activate," +
            "subscription.manage," +
            "traineegroup.manage,traineegroup.generate_sessions,session.manage," +
            "attendance.mark,attendance.view_rate";

        private const string AccountantPermissionsCsv =
            "payment.record,payment.correct,payment.refund,payment.view," +
            "finance.view," +
            "report.view,report.export," +
            "trainee.export";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the Employee role if this database doesn't have it yet.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'Employee')
                INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (NEWID(), N'Employee', N'EMPLOYEE', CONVERT(nvarchar(36), NEWID()));
            ");

            // 2. Repoint every user's role assignment: Manager -> Admin, Coach -> Employee,
            //    User -> Employee. Insert-then-skip-duplicates (AspNetUserRoles' PK is
            //    (UserId, RoleId)) so a user who already somehow holds both the old and new
            //    role doesn't break the migration.
            migrationBuilder.Sql(@"
                INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                SELECT ur.[UserId], target.[Id]
                FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetRoles] src ON src.[Id] = ur.[RoleId] AND src.[Name] = N'Manager'
                CROSS JOIN (SELECT [Id] FROM [AspNetRoles] WHERE [Name] = N'Admin') target
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AspNetUserRoles] existing
                    WHERE existing.[UserId] = ur.[UserId] AND existing.[RoleId] = target.[Id]);

                INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                SELECT ur.[UserId], target.[Id]
                FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetRoles] src ON src.[Id] = ur.[RoleId] AND src.[Name] = N'Coach'
                CROSS JOIN (SELECT [Id] FROM [AspNetRoles] WHERE [Name] = N'Employee') target
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AspNetUserRoles] existing
                    WHERE existing.[UserId] = ur.[UserId] AND existing.[RoleId] = target.[Id]);

                INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                SELECT ur.[UserId], target.[Id]
                FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetRoles] src ON src.[Id] = ur.[RoleId] AND src.[Name] = N'User'
                CROSS JOIN (SELECT [Id] FROM [AspNetRoles] WHERE [Name] = N'Employee') target
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AspNetUserRoles] existing
                    WHERE existing.[UserId] = ur.[UserId] AND existing.[RoleId] = target.[Id]);

                DELETE ur FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetRoles] r ON r.[Id] = ur.[RoleId]
                WHERE r.[Name] IN (N'Manager', N'Coach', N'User');
            ");

            // 3. Staff-onboarding invitations store the target role as a plain string
            //    (Invitations.Role) - remap pending ones the same way.
            migrationBuilder.Sql(@"
                UPDATE [Invitations] SET [Role] = N'Admin' WHERE [Role] = N'Manager';
                UPDATE [Invitations] SET [Role] = N'Employee' WHERE [Role] IN (N'Coach', N'User');
            ");

            // 4. Drop the three retired roles (their AspNetRoleClaims cascade/are removed first).
            migrationBuilder.Sql(@"
                DELETE rc FROM [AspNetRoleClaims] rc
                INNER JOIN [AspNetRoles] r ON r.[Id] = rc.[RoleId]
                WHERE r.[Name] IN (N'Manager', N'Coach', N'User');

                DELETE FROM [AspNetRoles] WHERE [Name] IN (N'Manager', N'Coach', N'User');
            ");

            // 5. Reconcile "permission" claims on the five surviving roles to
            //    AppDataSeeder.DefaultRolePermissions. Wipe first, then rebuild, so a role that
            //    picks up new permissions in this change (or loses one, like Admin losing
            //    tenant.users.manage) ends up exactly right regardless of what an older
            //    deployment had accumulated.
            migrationBuilder.Sql(@"
                DELETE rc FROM [AspNetRoleClaims] rc
                INNER JOIN [AspNetRoles] r ON r.[Id] = rc.[RoleId]
                WHERE r.[Name] IN (N'SuperAdmin', N'Owner', N'Admin', N'Employee', N'Accountant')
                  AND rc.[ClaimType] = N'permission';
            ");

            InsertRoleClaims(migrationBuilder, "SuperAdmin", AllPermissionsCsv);
            InsertRoleClaims(migrationBuilder, "Owner", AllPermissionsCsv, exclude: "platform.tenants.manage");
            InsertRoleClaims(migrationBuilder, "Admin", AllPermissionsCsv, exclude: "platform.tenants.manage,tenant.users.manage");
            InsertRoleClaims(migrationBuilder, "Employee", EmployeePermissionsCsv);
            InsertRoleClaims(migrationBuilder, "Accountant", AccountantPermissionsCsv);
        }

        private static void InsertRoleClaims(MigrationBuilder migrationBuilder, string roleName, string permissionsCsv, string exclude = "")
        {
            migrationBuilder.Sql($@"
                DECLARE @roleId uniqueidentifier = (SELECT [Id] FROM [AspNetRoles] WHERE [Name] = N'{roleName}');
                IF @roleId IS NOT NULL
                BEGIN
                    INSERT INTO [AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue])
                    SELECT @roleId, N'permission', p.[value]
                    FROM STRING_SPLIT(N'{permissionsCsv}', ',') p
                    WHERE p.[value] NOT IN (SELECT [value] FROM STRING_SPLIT(N'{exclude}', ','));
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreates the three retired roles (empty - no user re-assignment, see class
            // remarks) so a rollback doesn't leave AssignRolesToUser referencing role names
            // that no longer exist.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'Manager')
                INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (NEWID(), N'Manager', N'MANAGER', CONVERT(nvarchar(36), NEWID()));

                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'Coach')
                INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (NEWID(), N'Coach', N'COACH', CONVERT(nvarchar(36), NEWID()));

                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'User')
                INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (NEWID(), N'User', N'USER', CONVERT(nvarchar(36), NEWID()));
            ");
        }
    }
}
