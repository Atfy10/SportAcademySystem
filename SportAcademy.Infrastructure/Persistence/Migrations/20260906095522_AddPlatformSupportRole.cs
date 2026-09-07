using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // Adds the platform-only PlatformSupport role: read-only across the platform console
    // (platform.tenants.read, platform.audit.read - see AppDataSeeder.DefaultRolePermissions),
    // distinct from SuperAdmin which holds every platform.* permission and can mutate a tenant,
    // ban an owner, or impersonate into a tenant's account.
    //
    // The seeder itself is a no-op on any database that already has a tenant (see
    // AppDataSeeder.SeedAsync's early-return guard), so an existing deployment needs this
    // migration to create the role - same reasoning as ConsolidateRolesToFour and
    // GrantGranularPlatformPermissions.
    //
    // KNOWN GAP (tracked, not fixed here): there is no "platform users" management UI anywhere
    // in the app to assign this role to a second platform-level user - creating one today
    // requires a direct database operation. This migration only makes the role exist and be
    // correctly permissioned; provisioning a user into it is a separate, out-of-scope feature.
    public partial class AddPlatformSupportRole : Migration
    {
        private const string PlatformSupportPermissionsCsv = "platform.tenants.read,platform.audit.read";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'PlatformSupport')
                INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (NEWID(), N'PlatformSupport', N'PLATFORMSUPPORT', CONVERT(nvarchar(36), NEWID()));
            ");

            migrationBuilder.Sql(@"
                DECLARE @roleId uniqueidentifier = (SELECT [Id] FROM [AspNetRoles] WHERE [Name] = N'PlatformSupport');
                IF @roleId IS NOT NULL
                BEGIN
                    INSERT INTO [AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue])
                    SELECT @roleId, N'permission', p.[value]
                    FROM STRING_SPLIT(N'" + PlatformSupportPermissionsCsv + @"', ',') p
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
            // Only drops the role if nobody has been assigned to it (see ConsolidateRolesToFour's
            // Down for the same "never destroy a real user's role membership" reasoning) - the
            // permission claims cascade away with the role itself when it's deleted.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM [AspNetUserRoles] ur
                    INNER JOIN [AspNetRoles] r ON r.[Id] = ur.[RoleId]
                    WHERE r.[Name] = N'PlatformSupport')
                BEGIN
                    DELETE rc FROM [AspNetRoleClaims] rc
                    INNER JOIN [AspNetRoles] r ON r.[Id] = rc.[RoleId]
                    WHERE r.[Name] = N'PlatformSupport';

                    DELETE FROM [AspNetRoles] WHERE [Name] = N'PlatformSupport';
                END
            ");
        }
    }
}
