using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureIsImplementedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImplemented",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Catalog audit 2026-09-08 (see AppDataSeeder.FeatureCatalog's own comment for the
            // full reasoning per feature). Two groups:
            //  - Marked not-implemented, kept: real code/branch/product reason to expect these
            //    back, so the row (and every tenant's/plan's existing grant of it) stays intact.
            //  - Deleted: zero code, zero preserved branch, zero product-decision record backing
            //    them. Cascades to TenantFeatures and SubscriptionPlanFeatures via the existing
            //    FK configuration (SportAcademy.Infrastructure.Persistence.Configurations.
            //    FeatureConfiguration / SubscriptionPlanFeatureConfiguration) - no separate
            //    cleanup statements needed for those two tables.
            migrationBuilder.Sql(@"
                UPDATE Features
                SET IsImplemented = 0
                WHERE Name IN ('chat-system', 'ai-assistant', 'audit-trail', 'schedule-management');

                DELETE FROM Features
                WHERE Name IN ('api-access', 'health-test-mgmt', 'trainee-codes');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately does not restore the three deleted Features rows (or the
            // TenantFeature/SubscriptionPlanFeature rows their delete cascaded into) - that data
            // is gone once Up() runs, same as any other destructive migration. Down() here only
            // reverses the schema change; AppDataSeeder.ReconcileFeaturesAsync will re-add rows
            // for anything still listed in FeatureCatalog on the next startup, same as it does
            // for any other missing catalog entry.
            migrationBuilder.DropColumn(
                name: "IsImplemented",
                table: "Features");
        }
    }
}
