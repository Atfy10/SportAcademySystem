using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Backfills Arabic translations for the known seeded reference data - the 8 demo sports,
    /// 3 demo branches, the 2 payment types every tenant gets, the 12 demo trainee-group names,
    /// and the 9 global nationality categories.
    /// </summary>
    /// <remarks>
    /// Every insert is guarded by name/code match plus a NOT EXISTS on (EntityId, 'ar'), so this
    /// is safe to re-run and never overwrites a translation a tenant has already edited. Applies
    /// per-tenant via a join back to the base table, not to one hardcoded tenant id - so it
    /// covers every tenant whose data still matches the original seeded English names, and
    /// silently does nothing for rows that don't match (a tenant's own custom sport, a renamed
    /// branch), which is the intended fallback-to-English behaviour, not a bug.
    /// </remarks>
    public partial class SeedArabicReferenceDataTranslations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO SportTranslations (SportId, LangCode, Name, Description, TenantId)
                SELECT s.Id, N'ar', v.NameAr, v.DescAr, s.TenantId
                FROM Sports s
                JOIN (VALUES
                    (N'Swimming',     N'السباحة',        N'دروس سباحة احترافية وتدريب على السلامة المائية لجميع الأعمار'),
                    (N'Football',     N'كرة القدم',       N'اللعبة الجميلة - تدريب على كرة القدم ومباريات تنافسية'),
                    (N'Basketball',   N'كرة السلة',       N'تدريب كرة السلة داخل الصالات ومباريات تنافسية'),
                    (N'Volleyball',   N'الكرة الطائرة',    N'تدريب الكرة الطائرة الشاطئية والداخلية'),
                    (N'Tennis',       N'التنس',          N'تدريب احترافي على التنس لجميع المستويات'),
                    (N'Martial Arts', N'الفنون القتالية',  N'الكاراتيه والتايكوندو وتدريب الدفاع عن النفس'),
                    (N'Gymnastics',   N'الجمباز',         N'الجمباز الفني وتدريب المرونة'),
                    (N'Table Tennis', N'تنس الطاولة',      N'تدريب تنس الطاولة والبطولات')
                ) v(NameEn, NameAr, DescAr) ON s.Name = v.NameEn
                WHERE NOT EXISTS (
                    SELECT 1 FROM SportTranslations t WHERE t.SportId = s.Id AND t.LangCode = N'ar');
            ");

            migrationBuilder.Sql(@"
                INSERT INTO BranchTranslations (BranchId, LangCode, Name, City, Country, TenantId)
                SELECT b.Id, N'ar', v.NameAr, v.CityAr, N'الكويت', b.TenantId
                FROM Branches b
                JOIN (VALUES
                    (N'Salmiya Academy - Main Branch',    N'أكاديمية السالمية - الفرع الرئيسي', N'السالمية'),
                    (N'Salmiya Academy - Hawally Branch', N'أكاديمية السالمية - فرع حولي',      N'حولي'),
                    (N'Salmiya Academy - Jabriya Branch', N'أكاديمية السالمية - فرع الجابرية',   N'الجابرية')
                ) v(NameEn, NameAr, CityAr) ON b.Name = v.NameEn
                WHERE NOT EXISTS (
                    SELECT 1 FROM BranchTranslations t WHERE t.BranchId = b.Id AND t.LangCode = N'ar');
            ");

            // Cash/Online are seeded for every tenant (see AddPaymentTypes migration), so this
            // intentionally matches across all tenants rather than one.
            migrationBuilder.Sql(@"
                INSERT INTO PaymentTypeTranslations (PaymentTypeId, LangCode, Name, TenantId)
                SELECT p.Id, N'ar', v.NameAr, p.TenantId
                FROM PaymentTypes p
                JOIN (VALUES
                    (N'Cash',   N'نقدي'),
                    (N'Online', N'إلكتروني')
                ) v(NameEn, NameAr) ON p.Name = v.NameEn
                WHERE NOT EXISTS (
                    SELECT 1 FROM PaymentTypeTranslations t WHERE t.PaymentTypeId = p.Id AND t.LangCode = N'ar');
            ");

            migrationBuilder.Sql(@"
                INSERT INTO TraineeGroupTranslations (TraineeGroupId, LangCode, Name, TenantId)
                SELECT g.Id, N'ar', v.NameAr, g.TenantId
                FROM TraineeGroups g
                JOIN (VALUES
                    (N'Beginners A',       N'المبتدئون أ'),
                    (N'Beginners B',       N'المبتدئون ب'),
                    (N'Intermediate A',    N'المتوسطون أ'),
                    (N'Intermediate B',    N'المتوسطون ب'),
                    (N'Advanced A',        N'المتقدمون أ'),
                    (N'Advanced B',        N'المتقدمون ب'),
                    (N'Youth Development', N'تطوير الناشئين'),
                    (N'Junior Stars',      N'نجوم الناشئين'),
                    (N'Elite Squad',       N'الفريق النخبة'),
                    (N'Weekend Warriors',  N'محاربو نهاية الأسبوع'),
                    (N'Morning Session',   N'حصة صباحية'),
                    (N'Evening Session',   N'حصة مسائية')
                ) v(NameEn, NameAr) ON g.Name = v.NameEn
                WHERE NOT EXISTS (
                    SELECT 1 FROM TraineeGroupTranslations t WHERE t.TraineeGroupId = g.Id AND t.LangCode = N'ar');
            ");

            // Global lookup table, not tenant-scoped - matched by Code (unique), not name.
            migrationBuilder.Sql(@"
                INSERT INTO NationalityCategoryTranslations (NationalityCategoryId, LangCode, Name)
                SELECT nc.Id, N'ar', v.NameAr
                FROM NationalityCategories nc
                JOIN (VALUES
                    (N'KW',  N'كويتي'),
                    (N'GCC', N'مواطن خليجي'),
                    (N'AR',  N'عربي (خارج الخليج)'),
                    (N'AS',  N'آسيوي'),
                    (N'AF',  N'أفريقي'),
                    (N'EU',  N'أوروبي'),
                    (N'NA',  N'أمريكي شمالي'),
                    (N'SA',  N'أمريكي جنوبي'),
                    (N'OT',  N'أخرى')
                ) v(Code, NameAr) ON nc.Code = v.Code
                WHERE NOT EXISTS (
                    SELECT 1 FROM NationalityCategoryTranslations t WHERE t.NationalityCategoryId = nc.Id AND t.LangCode = N'ar');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM SportTranslations WHERE LangCode = N'ar';");
            migrationBuilder.Sql("DELETE FROM BranchTranslations WHERE LangCode = N'ar';");
            migrationBuilder.Sql("DELETE FROM PaymentTypeTranslations WHERE LangCode = N'ar';");
            migrationBuilder.Sql("DELETE FROM TraineeGroupTranslations WHERE LangCode = N'ar';");
            migrationBuilder.Sql("DELETE FROM NationalityCategoryTranslations WHERE LangCode = N'ar';");
        }
    }
}
