namespace SportAcademy.Application.Interfaces
{
    // Tenant-scoped, per-year, gap-free document numbering (INV-2026-00001, PAY-2026-00001, ...
    // and any future document type - CRN for credit notes, EXP for expenses - without touching
    // this interface). Backed by a single atomic counter increment
    // (usp_GenerateDocumentNumber) so concurrent recordings never collide or skip.
    public interface IFinancialDocumentNumberGenerator
    {
        Task<string> GenerateAsync(string documentType, CancellationToken ct = default);
    }
}
