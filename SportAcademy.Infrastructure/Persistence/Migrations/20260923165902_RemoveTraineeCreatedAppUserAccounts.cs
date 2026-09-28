using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // Data-only migration, no schema change: CreateTraineeCommandHandler used to create an
    // AppUser for every trainee, inserted straight through EF rather than
    // UserManager.CreateAsync - so it never got a SecurityStamp, and any later
    // UserManager.UpdateAsync on that row (activate/deactivate, edit) threw "User security
    // stamp cannot be null." That code path is now removed (nothing has ever signed in as a
    // trainee - there is no trainee-facing portal anywhere in the system), so this cleans up
    // the accounts it already created.
    //
    // A soft delete, not a hard DELETE: some of these rows may since have picked up
    // Restrict-FK'd children (a notification, a role) that would make a hard delete fail
    // outright depending on what actually happened to exist in a given database, and a soft
    // delete is what every other "delete a user" path in this app already does (see
    // SoftDeleteInterceptor) - this migration only reaches the same end state through SQL
    // instead of through that interceptor.
    public partial class RemoveTraineeCreatedAppUserAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @OrphanedUserIds TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);

                -- Every trainee-linked AppUser, except the vanishingly unlikely case where the
                -- same row somehow also belongs to an Employee - leave anything a real staff
                -- login might depend on strictly alone.
                INSERT INTO @OrphanedUserIds (Id)
                SELECT t.AppUserId
                FROM Trainees t
                WHERE t.AppUserId IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM Employees e WHERE e.AppUserId = t.AppUserId);

                UPDATE Trainees
                SET AppUserId = NULL
                WHERE AppUserId IN (SELECT Id FROM @OrphanedUserIds);

                -- AppUser maps to AspNetUsers, not "AppUsers" - IdentityDbContext's own base
                -- OnModelCreating names the table and wins over AppUserConfigurtion's ToTable
                -- call (confirmed against a real applied database, not assumed).
                UPDATE AspNetUsers
                SET IsDeleted = 1,
                    DeletedAt = SYSUTCDATETIME(),
                    DeletedBy = 'Migration:RemoveTraineeCreatedAppUserAccounts'
                WHERE Id IN (SELECT Id FROM @OrphanedUserIds)
                  AND IsDeleted = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible: which AppUserId used to belong to which Trainee is gone once
            // Up() nulls it out. Nothing in the schema changed, so there is nothing to revert
            // beyond the data itself.
        }
    }
}
