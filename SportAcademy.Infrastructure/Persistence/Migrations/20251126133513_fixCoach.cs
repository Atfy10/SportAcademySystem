using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixCoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Coaches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Coaches",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
