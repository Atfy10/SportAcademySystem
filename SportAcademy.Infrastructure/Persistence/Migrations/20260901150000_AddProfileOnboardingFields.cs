using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SportAcademy.Infrastructure.Persistence.DBContext;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    // Hand-authored: `dotnet ef migrations add` needs to build SportAcademy.Web, which was
    // locked by an active Visual Studio debug session for the whole session this was written
    // in. The [DbContext]/[Migration] attributes normally live on a generated *.Designer.cs
    // partial alongside a full BuildTargetModel() point-in-time snapshot; that partial is
    // omitted here (redundant with the already-updated ApplicationDbContextModelSnapshot.cs,
    // and thousands of lines to hand-transcribe with real risk of a transcription error) - the
    // attributes are placed directly on this class instead, which is all EF's migrator needs
    // to discover and apply it. Once a normal `dotnet ef migrations add` can run again, this
    // is safe to leave as-is; EF will simply diff future changes against the snapshot, which
    // already reflects these two columns.
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260901150000_AddProfileOnboardingFields")]
    public partial class AddProfileOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedOnboarding",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Profiles",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCompletedOnboarding",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Profiles");
        }
    }
}
