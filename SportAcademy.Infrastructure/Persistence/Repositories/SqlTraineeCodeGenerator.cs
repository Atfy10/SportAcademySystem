using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

public class SqlTraineeCodeGenerator : ITraineeCodeGenerator
{
    private readonly ApplicationDbContext _context;

    public SqlTraineeCodeGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(
        int familyId,
        int branchId,
        int nationalityCategoryId,
        AgeCategory ageCategory,
        CancellationToken cancellationToken)
    {
        var ageChar = ageCategory.ToChar();
        var ageCharString = ageChar.ToString();

        var result = new SqlParameter
        {
            ParameterName = "@TraineeCode",
            SqlDbType = System.Data.SqlDbType.NVarChar,
            Size = 50,
            Direction = System.Data.ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC usp_GenerateTraineeCode @FamilyId, @BranchId, @NationalityCategoryId, @AgeCode, @TraineeCode OUTPUT",
            new SqlParameter("@FamilyId", familyId),
            new SqlParameter("@BranchId", branchId),
            new SqlParameter("@NationalityCategoryId", nationalityCategoryId),
            new SqlParameter("@AgeCode", ageCharString),
            result,
            cancellationToken
        );

        return result.Value!.ToString()!;
    }
}