using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchToTrainees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'TraineeCatalog')
                    BEGIN
                        EXEC('CREATE FULLTEXT CATALOG TraineeCatalog');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'Trainees'
                    )
                    BEGIN
                        EXEC('CREATE FULLTEXT INDEX ON Trainees (FirstName LANGUAGE 1033, LastName LANGUAGE 1033, GuardianName LANGUAGE 1033, Email LANGUAGE 1033) KEY INDEX PK_Trainees ON TraineeCatalog WITH CHANGE_TRACKING AUTO');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF EXISTS (
                        SELECT 1 
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'Trainees'
                    )
                    BEGIN
                        EXEC('DROP FULLTEXT INDEX ON Trainees');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'TraineeCatalog')
                    BEGIN
                        EXEC('DROP FULLTEXT CATALOG TraineeCatalog');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);
        }
    }
}
