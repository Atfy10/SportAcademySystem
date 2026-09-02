using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTraineeCareerEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TraineeCareerEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SportId = table.Column<int>(type: "int", nullable: true),
                    TraineeGroupId = table.Column<int>(type: "int", nullable: true),
                    CoachId = table.Column<int>(type: "int", nullable: true),
                    EnrollmentId = table.Column<int>(type: "int", nullable: true),
                    SkillLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeCareerEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraineeCareerEvents_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeCareerEvents_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeCareerEvents_Sports_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeCareerEvents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeCareerEvents_TraineeGroups_TraineeGroupId",
                        column: x => x.TraineeGroupId,
                        principalTable: "TraineeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeCareerEvents_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_CoachId",
                table: "TraineeCareerEvents",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_EnrollmentId",
                table: "TraineeCareerEvents",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_SportId",
                table: "TraineeCareerEvents",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_TenantId",
                table: "TraineeCareerEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_TraineeGroupId",
                table: "TraineeCareerEvents",
                column: "TraineeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_TraineeId_EventType_EffectiveDate",
                table: "TraineeCareerEvents",
                columns: new[] { "TraineeId", "EventType", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCareerEvents_TraineeId_SportId_EventType_EffectiveDate",
                table: "TraineeCareerEvents",
                columns: new[] { "TraineeId", "SportId", "EventType", "EffectiveDate" });

            // Backfill 1: one SkillLevelChanged event per existing SportTrainee row.
            // SportTrainee has no CreatedAt (it implements only ITenantScoped, not
            // IAuditableEntity), so EffectiveDate falls back to the trainee's JoinDate for
            // every row - best-effort.
            migrationBuilder.Sql(@"
INSERT INTO TraineeCareerEvents (TraineeId, EventType, SportId, SkillLevel, EffectiveDate, CreatedAt, TenantId)
SELECT st.TraineeId, N'SkillLevelChanged', st.SportId, st.SkillLevel,
       CAST(tr.JoinDate AS datetime2), SYSUTCDATETIME(), st.TenantId
FROM SportTrainees st
JOIN Trainees tr ON tr.Id = st.TraineeId
WHERE NOT EXISTS (
    SELECT 1 FROM TraineeCareerEvents e
    WHERE e.TraineeId = st.TraineeId AND e.SportId = st.SportId AND e.EventType = N'SkillLevelChanged');
");

            // Backfill 2: one CoachAssigned event per currently-active Enrollment, coach
            // snapshotted from the enrollment's TraineeGroup.CoachId, dated at EnrollmentDate.
            migrationBuilder.Sql(@"
INSERT INTO TraineeCareerEvents
    (TraineeId, EventType, SportId, TraineeGroupId, CoachId, EnrollmentId, EffectiveDate, CreatedAt, TenantId)
SELECT en.TraineeId, N'CoachAssigned', c.SportId, en.TraineeGroupId, tg.CoachId, en.Id,
       CAST(en.EnrollmentDate AS datetime2), SYSUTCDATETIME(), en.TenantId
FROM Enrollments en
JOIN TraineeGroups tg ON tg.Id = en.TraineeGroupId
JOIN Coaches c ON c.EmployeeId = tg.CoachId
WHERE en.IsActive = 1 AND en.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM TraineeCareerEvents e WHERE e.EnrollmentId = en.Id AND e.EventType = N'CoachAssigned');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraineeCareerEvents");
        }
    }
}
