using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class releaseIdentityTraineeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_AspNetUsers_AppUserId",
                table: "Trainees");

            migrationBuilder.DropForeignKey(
                name: "FK_SportTrainees_Trainees_TraineeId",
                table: "SportTrainees");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Trainees_TraineeId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_Trainees_TraineeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropTable(
                name: "Trainees");

            migrationBuilder.CreateTable(
                name: "Trainees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SSN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    IsSubscribed = table.Column<bool>(type: "bit", nullable: false),
                    ParentNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trainees_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_SportTrainees_Trainees_TraineeId",
                table: "SportTrainees",
                column: "TraineeId",
                principalTable: "Trainees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Trainees_TraineeId",
                table: "Enrollments",
                column: "TraineeId",
                principalTable: "Trainees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_Trainees_TraineeId",
                table: "SubscriptionDetails",
                column: "TraineeId",
                principalTable: "Trainees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_AspNetUsers_AppUserId",
                table: "Trainees");

            migrationBuilder.DropForeignKey(
                name: "FK_SportTrainees_Trainees_TraineeId",
                table: "SportTrainees");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Trainees_TraineeId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_Trainees_TraineeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropTable(
                name: "Trainees");

            migrationBuilder.CreateTable(
                name: "Trainees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SSN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    IsSubscribed = table.Column<bool>(type: "bit", nullable: false),
                    ParentNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trainees_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_SportTrainees_Trainees_TraineeId",
                table: "SportTrainees",
                column: "TraineeId",
                principalTable: "Trainees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Trainees_TraineeId",
                table: "Enrollments",
                column: "TraineeId",
                principalTable: "Trainees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_Trainees_TraineeId",
                table: "SubscriptionDetails",
                column: "TraineeId",
                principalTable: "Trainees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }
    }
}
