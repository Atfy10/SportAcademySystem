using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TraineeGroupPauseAndExcuseRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSchedules_TraineeGroups_TraineeGroupId",
                table: "GroupSchedules");

            migrationBuilder.AddColumn<string>(
                name: "InactiveReason",
                table: "TraineeGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TraineeGroups",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "ExcuseRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionOccurrenceId = table.Column<int>(type: "int", nullable: false),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcuseRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcuseRequests_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExcuseRequests_SessionOccurrences_SessionOccurrenceId",
                        column: x => x.SessionOccurrenceId,
                        principalTable: "SessionOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExcuseRequests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExcuseRequests_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcuseRequests_EnrollmentId",
                table: "ExcuseRequests",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcuseRequests_SessionOccurrenceId",
                table: "ExcuseRequests",
                column: "SessionOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcuseRequests_TenantId",
                table: "ExcuseRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcuseRequests_TraineeId",
                table: "ExcuseRequests",
                column: "TraineeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSchedules_TraineeGroups_TraineeGroupId",
                table: "GroupSchedules",
                column: "TraineeGroupId",
                principalTable: "TraineeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSchedules_TraineeGroups_TraineeGroupId",
                table: "GroupSchedules");

            migrationBuilder.DropTable(
                name: "ExcuseRequests");

            migrationBuilder.DropColumn(
                name: "InactiveReason",
                table: "TraineeGroups");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TraineeGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSchedules_TraineeGroups_TraineeGroupId",
                table: "GroupSchedules",
                column: "TraineeGroupId",
                principalTable: "TraineeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
