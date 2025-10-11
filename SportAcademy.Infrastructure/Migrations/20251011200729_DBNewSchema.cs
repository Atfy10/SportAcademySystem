using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DBNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Sessions_SessionId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Enrollments",
                newName: "TraineeGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_SessionId",
                table: "Enrollments",
                newName: "IX_Enrollments_TraineeGroupId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SessionAllowed",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionRemaining",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionOccurrenceId",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TraineeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaximumCapacity = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    DurationInMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 55),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CoachId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraineeGroups_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeGroups_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeGroupId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupSchedules_TraineeGroups_TraineeGroupId",
                        column: x => x.TraineeGroupId,
                        principalTable: "TraineeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionOccurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupScheduleId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionOccurrences_GroupSchedules_GroupScheduleId",
                        column: x => x.GroupScheduleId,
                        principalTable: "GroupSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Name",
                table: "Branches",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_SessionOccurrenceId",
                table: "Attendances",
                column: "SessionOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSchedules_TraineeGroupId",
                table: "GroupSchedules",
                column: "TraineeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionOccurrences_GroupScheduleId",
                table: "SessionOccurrences",
                column: "GroupScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeGroups_BranchId",
                table: "TraineeGroups",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeGroups_CoachId",
                table: "TraineeGroups",
                column: "CoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_SessionOccurrences_SessionOccurrenceId",
                table: "Attendances",
                column: "SessionOccurrenceId",
                principalTable: "SessionOccurrences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_TraineeGroups_TraineeGroupId",
                table: "Enrollments",
                column: "TraineeGroupId",
                principalTable: "TraineeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_SessionOccurrences_SessionOccurrenceId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_TraineeGroups_TraineeGroupId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "SessionOccurrences");

            migrationBuilder.DropTable(
                name: "GroupSchedules");

            migrationBuilder.DropTable(
                name: "TraineeGroups");

            migrationBuilder.DropIndex(
                name: "IX_Branch_Name",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_SessionOccurrenceId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "SessionAllowed",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "SessionRemaining",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "SessionOccurrenceId",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "TraineeGroupId",
                table: "Enrollments",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_TraineeGroupId",
                table: "Enrollments",
                newName: "IX_Enrollments_SessionId");

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CoachId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Day = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    DurationInMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 55),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaximumCapacity = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    SkillLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_BranchId",
                table: "Sessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CoachId",
                table: "Sessions",
                column: "CoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Sessions_SessionId",
                table: "Enrollments",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
