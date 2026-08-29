using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// A SQL Server scalar function folding the common Arabic spelling variants (alef-hamza
    /// forms, taa marbuta, alef maqsura, tatweel) so LIKE-based search can match "أحمد" against
    /// "احمد" - the same name, spelled two ways a user reasonably expects to both work.
    /// </summary>
    /// <remarks>
    /// Mirrors SportAcademy.Domain.Helpers.ArabicTextNormalizer, but only its four most common
    /// foldings - harakat (diacritic) stripping is deliberately not included here: it would need
    /// ~40 additional chained REPLACE calls for a case search terms essentially never contain
    /// (nobody types diacritics into a name search box), whereas the C# normalizer handles the
    /// full set for the few callers that do need it. Applied at the SQL Server session/deployment
    /// level rather than as a persisted computed column, since the reference tables this backs
    /// (Trainees) are searched via ad hoc raw SQL, not a fixed schema shape.
    /// </remarks>
    public partial class AddArabicNormalizationFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER FUNCTION dbo.NormalizeArabicText(@input NVARCHAR(MAX))
                RETURNS NVARCHAR(MAX)
                WITH SCHEMABINDING
                AS
                BEGIN
                    IF @input IS NULL RETURN NULL;
                    RETURN
                        REPLACE(REPLACE(REPLACE(
                        REPLACE(REPLACE(REPLACE(REPLACE(
                            @input,
                            NCHAR(0x0623), NCHAR(0x0627)), -- hamza-above alef -> bare alef
                            NCHAR(0x0625), NCHAR(0x0627)), -- hamza-below alef -> bare alef
                            NCHAR(0x0622), NCHAR(0x0627)), -- madda alef -> bare alef
                            NCHAR(0x0671), NCHAR(0x0627)), -- wasla alef -> bare alef
                            NCHAR(0x0629), NCHAR(0x0647)), -- taa marbuta -> haa
                            NCHAR(0x0649), NCHAR(0x064A)), -- alef maqsura -> yaa
                            NCHAR(0x0640), N'');            -- tatweel -> removed
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.NormalizeArabicText;");
        }
    }
}
