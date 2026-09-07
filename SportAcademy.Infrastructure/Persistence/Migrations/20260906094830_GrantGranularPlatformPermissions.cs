using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    // Splits the single platform.tenants.manage permission (SuperAdmin-only, per
    // AppDataSeeder.DefaultRolePermissions) into five granular platform.* permissions -
    // Permissions.Platform in the domain catalog is the source of truth for the names. The
    // seeder itself is a no-op on any database that already has a tenant (see
    // AppDataSeeder.SeedAsync's early-return guard), so an existing deployment's SuperAdmin
    // role claims need this data migration to pick up the four newly-added permissions
    // (TenantsRead, OwnersManage, AuditRead, Impersonate) - same reasoning as the
    // ConsolidateRolesToFour migration's role-claim reconciliation.
    //
    // No other seeded role (Owner/Admin/Employee/Accountant) has ever held any platform.*
    // permission (DefaultRolePermissions filters them out via !p.StartsWith("platform.")), so
    // only SuperAdmin needs reconciling here.
    public partial class GrantGranularPlatformPermissions : Migration
    {
        private const string NewPlatformPermissionsCsv =
            "platform.tenants.read,platform.owners.manage,platform.audit.read,platform.impersonate";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @roleId uniqueidentifier = (SELECT [Id] FROM [AspNetRoles] WHERE [Name] = N'SuperAdmin');
                IF @roleId IS NOT NULL
                BEGIN
                    INSERT INTO [AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue])
                    SELECT @roleId, N'permission', p.[value]
                    FROM STRING_SPLIT(N'" + NewPlatformPermissionsCsv + @"', ',') p
                    WHERE NOT EXISTS (
                        SELECT 1 FROM [AspNetRoleClaims] existing
                        WHERE existing.[RoleId] = @roleId
                          AND existing.[ClaimType] = N'permission'
                          AND existing.[ClaimValue] = p.[value]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE rc FROM [AspNetRoleClaims] rc
                INNER JOIN [AspNetRoles] r ON r.[Id] = rc.[RoleId]
                WHERE r.[Name] = N'SuperAdmin'
                  AND rc.[ClaimType] = N'permission'
                  AND rc.[ClaimValue] IN (SELECT [value] FROM STRING_SPLIT(N'" + NewPlatformPermissionsCsv + @"', ','));
            ");
        }
    }
}
