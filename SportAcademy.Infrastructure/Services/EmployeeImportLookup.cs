using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Services
{
    public class EmployeeImportLookup : IEmployeeImportLookup
    {
        private readonly ApplicationDbContext _context;

        public EmployeeImportLookup(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyList<ImportNamedItem>> GetBranchesAsync(CancellationToken ct = default)
            => (await _context.Branchs.AsNoTracking()
                    .Select(b => new { b.Id, b.Name, b.IsActive, Alt = b.Translations.Select(t => t.Name).ToList() })
                    .ToListAsync(ct))
                .Select(b => new ImportNamedItem(b.Id, b.Name, b.Alt, b.IsActive))
                .ToList();

        public async Task<ExistingEmployeeIdentifiers> GetExistingIdentifiersAsync(
            IEnumerable<string> phones, IEnumerable<string> ssns, CancellationToken ct = default)
        {
            var phoneList = phones.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
            var ssnList = ssns.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            // Same predicates as EmployeeRepository.IsPhoneNumberExistAsync / IsSSNExistAsync
            // (what CreateEmployeeCommandHandler rejects on), just set-based.
            var existingPhones = phoneList.Count == 0 ? [] : await _context.Employees.AsNoTracking()
                .Where(e => phoneList.Contains(e.PhoneNumber)).Select(e => e.PhoneNumber).ToListAsync(ct);
            var existingSsns = ssnList.Count == 0 ? [] : await _context.Employees.AsNoTracking()
                .Where(e => ssnList.Contains(e.SSN)).Select(e => e.SSN).ToListAsync(ct);

            return new ExistingEmployeeIdentifiers(existingPhones.ToHashSet(), existingSsns.ToHashSet());
        }

        public EmployeeFieldLimits GetFieldLimits()
        {
            var employee = _context.Model.FindEntityType(typeof(Employee))!;
            int Len(string property, int fallback) => employee.FindProperty(property)?.GetMaxLength() ?? fallback;

            int Owned(string navigation, string property, int fallback)
                => employee.FindNavigation(navigation)?.TargetEntityType.FindProperty(property)?.GetMaxLength() ?? fallback;

            return new EmployeeFieldLimits(
                FirstName: Len(nameof(Employee.FirstName), 50),
                LastName: Len(nameof(Employee.LastName), 50),
                Ssn: Len(nameof(Employee.SSN), 20),
                PhoneNumber: Len(nameof(Employee.PhoneNumber), 20),
                SecondPhoneNumber: Len(nameof(Employee.SecondPhoneNumber), 20),
                Email: Owned(nameof(Employee.Email), "Value", 200),
                Street: Owned(nameof(Employee.Address), "Street", 70),
                City: Owned(nameof(Employee.Address), "City", 50));
        }
    }
}
