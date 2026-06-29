using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationsAndTenantArchival : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "VideoAnalyses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SportTrainees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SportSubscriptionTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SportPrices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SportBranches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ChatConversations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReplacedByInvitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyses_TenantId",
                table: "VideoAnalyses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SportTrainees_TenantId",
                table: "SportTrainees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SportSubscriptionTypes_TenantId",
                table: "SportSubscriptionTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SportPrices_TenantId",
                table: "SportPrices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SportBranches_TenantId",
                table: "SportBranches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_TenantId",
                table: "ChatMessages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_TenantId",
                table: "ChatConversations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TenantId_Email_Status",
                table: "Invitations",
                columns: new[] { "TenantId", "Email", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TokenHash",
                table: "Invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConversations_Tenants_TenantId",
                table: "ChatConversations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Tenants_TenantId",
                table: "ChatMessages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportBranches_Tenants_TenantId",
                table: "SportBranches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportPrices_Tenants_TenantId",
                table: "SportPrices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportSubscriptionTypes_Tenants_TenantId",
                table: "SportSubscriptionTypes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportTrainees_Tenants_TenantId",
                table: "SportTrainees",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoAnalyses_Tenants_TenantId",
                table: "VideoAnalyses",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatConversations_Tenants_TenantId",
                table: "ChatConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Tenants_TenantId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_SportBranches_Tenants_TenantId",
                table: "SportBranches");

            migrationBuilder.DropForeignKey(
                name: "FK_SportPrices_Tenants_TenantId",
                table: "SportPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SportSubscriptionTypes_Tenants_TenantId",
                table: "SportSubscriptionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_SportTrainees_Tenants_TenantId",
                table: "SportTrainees");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoAnalyses_Tenants_TenantId",
                table: "VideoAnalyses");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_VideoAnalyses_TenantId",
                table: "VideoAnalyses");

            migrationBuilder.DropIndex(
                name: "IX_SportTrainees_TenantId",
                table: "SportTrainees");

            migrationBuilder.DropIndex(
                name: "IX_SportSubscriptionTypes_TenantId",
                table: "SportSubscriptionTypes");

            migrationBuilder.DropIndex(
                name: "IX_SportPrices_TenantId",
                table: "SportPrices");

            migrationBuilder.DropIndex(
                name: "IX_SportBranches_TenantId",
                table: "SportBranches");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_TenantId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatConversations_TenantId",
                table: "ChatConversations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VideoAnalyses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SportTrainees");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SportSubscriptionTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SportPrices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SportBranches");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ChatConversations");
        }
    }
}
