using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureBundlePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BundlePrice",
                table: "Features",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsBundleCore",
                table: "Features",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // One-time backfill for feature rows that already existed before this migration ran
            // (any non-empty database) - AppDataSeeder.BundlePricingDefaults only seeds these
            // values for a row it's creating fresh, so without this an already-deployed database
            // would show every bundle-builder price as $0 until a SuperAdmin manually re-entered
            // all 25. Matches AppDataSeeder.BundlePricingDefaults exactly; a future change to
            // those defaults does not need a matching migration (that map only affects newly-
            // created features from then on) - this UPDATE is a one-time launch backfill, not an
            // ongoing sync path.
            migrationBuilder.Sql("""
                UPDATE Features SET BundlePrice = 0, IsBundleCore = 1 WHERE Name IN
                    ('user-management','role-management','tenant-settings','profile-mgmt','system-settings',
                     'branch-management','employee-management','coach-management','sport-management');
                UPDATE Features SET BundlePrice = 35, IsBundleCore = 1 WHERE Name = 'trainee-management';
                UPDATE Features SET BundlePrice = 10, IsBundleCore = 0 WHERE Name = 'subscription-plan';
                UPDATE Features SET BundlePrice = 8,  IsBundleCore = 0 WHERE Name = 'pricing-management';
                UPDATE Features SET BundlePrice = 15, IsBundleCore = 0 WHERE Name = 'payment-processing';
                UPDATE Features SET BundlePrice = 10, IsBundleCore = 0 WHERE Name = 'group-management';
                UPDATE Features SET BundlePrice = 12, IsBundleCore = 0 WHERE Name = 'attendance-tracking';
                UPDATE Features SET BundlePrice = 12, IsBundleCore = 0 WHERE Name = 'enrollment-management';
                UPDATE Features SET BundlePrice = 8,  IsBundleCore = 0 WHERE Name = 'family-management';
                UPDATE Features SET BundlePrice = 3,  IsBundleCore = 0 WHERE Name = 'nationality-categories';
                UPDATE Features SET BundlePrice = 10, IsBundleCore = 0 WHERE Name = 'financial-reports';
                UPDATE Features SET BundlePrice = 8,  IsBundleCore = 0 WHERE Name = 'attendance-reports';
                UPDATE Features SET BundlePrice = 8,  IsBundleCore = 0 WHERE Name = 'subscription-reports';
                UPDATE Features SET BundlePrice = 6,  IsBundleCore = 0 WHERE Name = 'notifications';
                UPDATE Features SET BundlePrice = 7,  IsBundleCore = 0 WHERE Name = 'discount-offers';
                UPDATE Features SET BundlePrice = 10, IsBundleCore = 0 WHERE Name = 'session-management';
                UPDATE Features SET BundlePrice = 6,  IsBundleCore = 0 WHERE Name = 'backup-restore';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BundlePrice",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "IsBundleCore",
                table: "Features");
        }
    }
}
