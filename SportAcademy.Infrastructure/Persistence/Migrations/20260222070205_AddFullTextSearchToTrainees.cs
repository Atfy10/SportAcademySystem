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
                IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'TraineeCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG TraineeCatalog;
                END
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Trainees'
                )
                BEGIN
                    CREATE FULLTEXT INDEX ON Trainees
                    (
                        FirstName LANGUAGE 1033,
                        LastName LANGUAGE 1033,
                        GuardianName LANGUAGE 1033,
                        Email LANGUAGE 1033
                    )
                    KEY INDEX PK_Trainees
                    ON TraineeCatalog
                    WITH CHANGE_TRACKING AUTO;
                END
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Trainees'
                )
                BEGIN
                    DROP FULLTEXT INDEX ON Trainees;
                END
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'TraineeCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG TraineeCatalog;
                END
            ", suppressTransaction: true);
        }
    }
}
