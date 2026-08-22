using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SqlFinancialDocumentNumberGenerator : IFinancialDocumentNumberGenerator
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantIdProvider _tenantIdProvider;

        public SqlFinancialDocumentNumberGenerator(ApplicationDbContext context, ITenantIdProvider tenantIdProvider)
        {
            _context = context;
            _tenantIdProvider = tenantIdProvider;
        }

        public async Task<string> GenerateAsync(string documentType, CancellationToken ct = default)
        {
            var result = new SqlParameter
            {
                ParameterName = "@DocumentNumber",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = 50,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_GenerateDocumentNumber @TenantId, @DocumentType, @Year, @DocumentNumber OUTPUT",
                new SqlParameter("@TenantId", _tenantIdProvider.TenantId),
                new SqlParameter("@DocumentType", documentType),
                new SqlParameter("@Year", DateTime.UtcNow.Year),
                result
            );

            return result.Value!.ToString()!;
        }
    }
}
