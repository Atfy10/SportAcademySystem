using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addRelationEnrollmentToSubDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubscriptionDetailsId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "CheckInTime",
                table: "Attendances",
                type: "time",
                nullable: false,
                defaultValueSql: "CONVERT(TIME, GETUTCDATE())",
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AttendanceDate",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_SubscriptionDetailsId",
                table: "Enrollments",
                column: "SubscriptionDetailsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_SubscriptionDetails_SubscriptionDetailsId",
                table: "Enrollments",
                column: "SubscriptionDetailsId",
                principalTable: "SubscriptionDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_SubscriptionDetails_SubscriptionDetailsId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_SubscriptionDetailsId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "SubscriptionDetailsId",
                table: "Enrollments");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "CheckInTime",
                table: "Attendances",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldDefaultValueSql: "CONVERT(TIME, GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AttendanceDate",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
