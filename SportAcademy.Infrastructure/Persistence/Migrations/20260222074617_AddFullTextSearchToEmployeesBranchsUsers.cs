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
                BEGIN TRY
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'MainFullTextCatalog'
                    )
                    BEGIN
                        EXEC('CREATE FULLTEXT CATALOG MainFullTextCatalog');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            // ===================== Employees =====================
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF NOT EXISTS (
                        SELECT 1
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'Employees'
                    )
                    BEGIN
                        EXEC('CREATE FULLTEXT INDEX ON dbo.Employees (FirstName LANGUAGE 1033, LastName LANGUAGE 1033, PhoneNumber LANGUAGE 1033) KEY INDEX PK_Employees ON MainFullTextCatalog WITH CHANGE_TRACKING AUTO');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            // ===================== AspNetUsers =====================
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF NOT EXISTS (
                        SELECT 1
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'AspNetUsers'
                    )
                    BEGIN
                        EXEC('CREATE FULLTEXT INDEX ON dbo.AspNetUsers (UserName LANGUAGE 1033, Email LANGUAGE 1033) KEY INDEX PK_AspNetUsers ON MainFullTextCatalog WITH CHANGE_TRACKING AUTO');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            // ===================== Branches =====================
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF NOT EXISTS (
                        SELECT 1
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'Branches'
                    )
                    BEGIN
                        EXEC('CREATE FULLTEXT INDEX ON dbo.Branches (Name LANGUAGE 1033, City LANGUAGE 1033, Country LANGUAGE 1033, Email LANGUAGE 1033) KEY INDEX PK_Branches ON MainFullTextCatalog WITH CHANGE_TRACKING AUTO');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);
        }
        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Employees
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF EXISTS (
                        SELECT 1
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'Employees'
                    )
                    BEGIN
                        EXEC('DROP FULLTEXT INDEX ON dbo.Employees');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            // AspNetUsers
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF EXISTS (
                        SELECT 1
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'AspNetUsers'
                    )
                    BEGIN
                        EXEC('DROP FULLTEXT INDEX ON dbo.AspNetUsers');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            // Branches
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF EXISTS (
                        SELECT 1
                        FROM sys.fulltext_indexes fi
                        JOIN sys.objects o ON fi.object_id = o.object_id
                        WHERE o.name = 'Branches'
                    )
                    BEGIN
                        EXEC('DROP FULLTEXT INDEX ON dbo.Branches');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);

            // Drop catalog (only if no indexes remain)
            migrationBuilder.Sql(@"
                BEGIN TRY
                    IF EXISTS (
                        SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'MainFullTextCatalog'
                    )
                    BEGIN
                        EXEC('DROP FULLTEXT CATALOG MainFullTextCatalog');
                    END
                END TRY
                BEGIN CATCH
                END CATCH
            ", suppressTransaction: true);
        }
    }
}
