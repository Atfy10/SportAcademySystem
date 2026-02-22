using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchToEmployeesBranchsUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create main catalog
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'MainFullTextCatalog'
                )
                BEGIN
                    CREATE FULLTEXT CATALOG MainFullTextCatalog;
                END
            ", suppressTransaction: true);

            // ===================== Employees =====================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Employees'
                )
                BEGIN
                    CREATE FULLTEXT INDEX ON dbo.Employees
                    (
                        FirstName LANGUAGE 1033,
                        LastName LANGUAGE 1033,
                        PhoneNumber LANGUAGE 1033
                    )
                    KEY INDEX PK_Employees
                    ON MainFullTextCatalog
                    WITH CHANGE_TRACKING AUTO;
                END
            ", suppressTransaction: true);

            // ===================== AspNetUsers =====================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'AspNetUsers'
                )
                BEGIN
                    CREATE FULLTEXT INDEX ON dbo.AspNetUsers
                    (
                        UserName LANGUAGE 1033,
                        Email LANGUAGE 1033
                    )
                    KEY INDEX PK_AspNetUsers
                    ON MainFullTextCatalog
                    WITH CHANGE_TRACKING AUTO;
                END
            ", suppressTransaction: true);

            // ===================== Branches =====================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Branches'
                )
                BEGIN
                    CREATE FULLTEXT INDEX ON dbo.Branches
                    (
                        Name LANGUAGE 1033,
                        City LANGUAGE 1033,
                        Country LANGUAGE 1033,
                        Email LANGUAGE 1033
                    )
                    KEY INDEX PK_Branches
                    ON MainFullTextCatalog
                    WITH CHANGE_TRACKING AUTO;
                END
            ", suppressTransaction: true);
        }
        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Employees
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Employees'
                )
                BEGIN
                    DROP FULLTEXT INDEX ON dbo.Employees;
                END
            ", suppressTransaction: true);

            // AspNetUsers
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'AspNetUsers'
                )
                BEGIN
                    DROP FULLTEXT INDEX ON dbo.AspNetUsers;
                END
            ", suppressTransaction: true);

            // Branches
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Branches'
                )
                BEGIN
                    DROP FULLTEXT INDEX ON dbo.Branches;
                END
            ", suppressTransaction: true);

            // Drop catalog (only if no indexes remain)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'MainFullTextCatalog'
                )
                BEGIN
                    DROP FULLTEXT CATALOG MainFullTextCatalog;
                END
            ", suppressTransaction: true);
        }
    }
}
