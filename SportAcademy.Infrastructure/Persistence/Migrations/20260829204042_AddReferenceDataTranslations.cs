using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceDataTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchTranslations",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    LangCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchTranslations", x => new { x.BranchId, x.LangCode });
                    table.ForeignKey(
                        name: "FK_BranchTranslations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchTranslations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NationalityCategoryTranslations",
                columns: table => new
                {
                    NationalityCategoryId = table.Column<int>(type: "int", nullable: false),
                    LangCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalityCategoryTranslations", x => new { x.NationalityCategoryId, x.LangCode });
                    table.ForeignKey(
                        name: "FK_NationalityCategoryTranslations_NationalityCategories_NationalityCategoryId",
                        column: x => x.NationalityCategoryId,
                        principalTable: "NationalityCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypeTranslations",
                columns: table => new
                {
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    LangCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypeTranslations", x => new { x.PaymentTypeId, x.LangCode });
                    table.ForeignKey(
                        name: "FK_PaymentTypeTranslations_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTypeTranslations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SportTranslations",
                columns: table => new
                {
                    SportId = table.Column<int>(type: "int", nullable: false),
                    LangCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportTranslations", x => new { x.SportId, x.LangCode });
                    table.ForeignKey(
                        name: "FK_SportTranslations_Sports_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportTranslations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TraineeGroupTranslations",
                columns: table => new
                {
                    TraineeGroupId = table.Column<int>(type: "int", nullable: false),
                    LangCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeGroupTranslations", x => new { x.TraineeGroupId, x.LangCode });
                    table.ForeignKey(
                        name: "FK_TraineeGroupTranslations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraineeGroupTranslations_TraineeGroups_TraineeGroupId",
                        column: x => x.TraineeGroupId,
                        principalTable: "TraineeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchTranslations_TenantId",
                table: "BranchTranslations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypeTranslations_TenantId",
                table: "PaymentTypeTranslations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SportTranslations_TenantId",
                table: "SportTranslations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeGroupTranslations_TenantId",
                table: "TraineeGroupTranslations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchTranslations");

            migrationBuilder.DropTable(
                name: "NationalityCategoryTranslations");

            migrationBuilder.DropTable(
                name: "PaymentTypeTranslations");

            migrationBuilder.DropTable(
                name: "SportTranslations");

            migrationBuilder.DropTable(
                name: "TraineeGroupTranslations");
        }
    }
}
