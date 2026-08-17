using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddViewsAndCodeGeneratorSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlFile = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "Sql",
                "Procedures",
                "usp_GenerateTraineeCode.sql");

            var sql = File.ReadAllText(sqlFile);

            migrationBuilder.Sql(sql, suppressTransaction: true);

            var views = new[]
            {
                "vw_AdminBasic.sql",
                "vw_CoachSchedule.sql",
                "vw_CoachSkill.sql",
                "vw_EmployeeBasic.sql",
                "vw_EmployeeWork.sql",
                "vw_GroupsView.sql",
                "vw_ScheduleDaily.sql",
                "vw_ScheduleWeekly.sql",
                "vw_TraineeAttendance.sql",
                "vw_TraineeBasic.sql",
                "vw_TraineeSchedule.sql",
                "vw_TraineeSession.sql",
                "vw_TraineeSubscription.sql"
            };

            foreach (var view in views)
            {
                migrationBuilder.Sql(
                    File.ReadAllText(
                        Path.Combine(
                            AppContext.BaseDirectory,
                            "Persistence",
                            "Sql",
                            "Views",
                            view)));
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.usp_GenerateTraineeCode', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.usp_GenerateTraineeCode;
                """);

            var views = new[]
            {
                "vw_AdminBasic",
                "vw_CoachSchedule",
                "vw_CoachSkill",
                "vw_EmployeeBasic",
                "vw_EmployeeWork",
                "vw_GroupsView",
                "vw_ScheduleDaily",
                "vw_ScheduleWeekly",
                "vw_TraineeAttendance",
                "vw_TraineeBasic",
                "vw_TraineeSchedule",
                "vw_TraineeSession",
                "vw_TraineeSubscription"
            };

            foreach (var view in views)
            {
                migrationBuilder.Sql($"""
                IF OBJECT_ID('dbo.{view}', 'V') IS NOT NULL
                    DROP VIEW dbo.{view};
                """);
            }
        }
    }
}
