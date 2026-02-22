using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class alterEmployeeFullText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.fulltext_indexes fi
                    JOIN sys.objects o ON fi.object_id = o.object_id
                    WHERE o.name = 'Employees'
                )
                BEGIN
                    ALTER FULLTEXT INDEX ON dbo.Employees
                    ADD (Email LANGUAGE 1033);
                END
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.fulltext_index_columns ic
                    JOIN sys.columns c 
                      ON ic.object_id = c.object_id 
                     AND ic.column_id = c.column_id
                    JOIN sys.objects o 
                      ON o.object_id = ic.object_id
                    WHERE o.name = 'Employees'
                      AND c.name = 'SecondPhoneNumber'
                )
                BEGIN
                    ALTER FULLTEXT INDEX ON dbo.Employees
                    DROP (SecondPhoneNumber);
                END
            ", suppressTransaction: true);
        }
    }
}
