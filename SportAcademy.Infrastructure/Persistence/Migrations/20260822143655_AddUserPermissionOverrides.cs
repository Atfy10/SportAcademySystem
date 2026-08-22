using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPermissionOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPermissionOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Effect = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_TenantId",
                table: "UserPermissionOverrides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_UserId_Permission",
                table: "UserPermissionOverrides",
                columns: new[] { "UserId", "Permission" },
                unique: true);

            // Backfill: every per-user "permission" claim previously lived in AspNetUserClaims
            // (grant-only - there was no way to express a Deny). PermissionResolver now reads
            // exclusively from UserPermissionOverrides, so translate each claim into an
            // equivalent Allow override before the old claim rows are removed. DISTINCT guards
            // against the unique (UserId, Permission) index above in case a user somehow
            // accumulated the same claim twice.
            migrationBuilder.Sql(@"
                INSERT INTO [UserPermissionOverrides] ([TenantId], [UserId], [Permission], [Effect], [CreatedAt], [CreatedBy])
                SELECT DISTINCT u.[TenantId], c.[UserId], c.[ClaimValue], N'Allow', GETUTCDATE(), N'migration:AddUserPermissionOverrides'
                FROM [AspNetUserClaims] c
                INNER JOIN [AspNetUsers] u ON u.[Id] = c.[UserId]
                WHERE c.[ClaimType] = N'permission';

                DELETE FROM [AspNetUserClaims] WHERE [ClaimType] = N'permission';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPermissionOverrides");
        }
    }
}
