using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Services
{
    public class TraineeImportLookup : ITraineeImportLookup
    {
        private readonly ApplicationDbContext _context;

        public TraineeImportLookup(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyList<ImportNamedItem>> GetBranchesAsync(CancellationToken ct = default)
            => (await _context.Branchs.AsNoTracking()
                    .Select(b => new { b.Id, b.Name, b.IsActive, Alt = b.Translations.Select(t => t.Name).ToList() })
                    .ToListAsync(ct))
                .Select(b => new ImportNamedItem(b.Id, b.Name, b.Alt, b.IsActive))
                .ToList();

        public async Task<IReadOnlyList<ImportNamedItem>> GetNationalityCategoriesAsync(CancellationToken ct = default)
            => (await _context.NationalityCategories.AsNoTracking()
                    .Select(n => new { n.Id, n.Name, n.Code, Alt = n.Translations.Select(t => t.Name).ToList() })
                    .ToListAsync(ct))
                .Select(n => new ImportNamedItem(n.Id, n.Name, n.Alt.Append(n.Code).ToList(), true))
                .ToList();

        public async Task<IReadOnlyList<ImportNamedItem>> GetSportsAsync(CancellationToken ct = default)
            => (await _context.Sports.AsNoTracking()
                    .Select(s => new { s.Id, s.Name, s.IsActive, Alt = s.Translations.Select(t => t.Name).ToList() })
                    .ToListAsync(ct))
                .Select(s => new ImportNamedItem(s.Id, s.Name, s.Alt, s.IsActive))
                .ToList();

        public async Task<IReadOnlySet<int>> GetExistingFamilyIdsAsync(IEnumerable<int> familyIds, CancellationToken ct = default)
        {
            var ids = familyIds.Distinct().ToList();
            if (ids.Count == 0) return new HashSet<int>();
            return (await _context.Families.AsNoTracking()
                    .Where(f => ids.Contains(f.Id))
                    .Select(f => f.Id)
                    .ToListAsync(ct))
                .ToHashSet();
        }

        public async Task<ExistingTraineeIdentifiers> GetExistingIdentifiersAsync(
            IEnumerable<string> phones, IEnumerable<string> ssns, IEnumerable<string> emails, CancellationToken ct = default)
        {
            var phoneList = phones.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
            var ssnList = ssns.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            var emailList = emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.ToLowerInvariant()).Distinct().ToList();

            // Same predicates as TraineeRepository.IsPhoneNumberExistAsync / IsSSNExistAsync /
            // IsEmailExistAsync (what CreateTraineeCommandHandler rejects on), just set-based.
            var existingPhones = phoneList.Count == 0 ? [] : await _context.Trainees.AsNoTracking()
                .Where(t => phoneList.Contains(t.PhoneNumber)).Select(t => t.PhoneNumber).ToListAsync(ct);
            var existingSsns = ssnList.Count == 0 ? [] : await _context.Trainees.AsNoTracking()
                .Where(t => ssnList.Contains(t.SSN)).Select(t => t.SSN).ToListAsync(ct);
            var existingEmails = emailList.Count == 0 ? [] : await _context.Trainees.AsNoTracking()
                .Where(t => emailList.Contains(t.Email.Value)).Select(t => t.Email.Value).ToListAsync(ct);

            return new ExistingTraineeIdentifiers(
                existingPhones.ToHashSet(),
                existingSsns.ToHashSet(),
                existingEmails.Select(e => e.ToLowerInvariant()).ToHashSet());
        }

        public TraineeFieldLimits GetFieldLimits()
        {
            var trainee = _context.Model.FindEntityType(typeof(Trainee))!;
            int Len(string property, int fallback) => trainee.FindProperty(property)?.GetMaxLength() ?? fallback;

            int Owned(string navigation, string property, int fallback)
                => trainee.FindNavigation(navigation)?.TargetEntityType.FindProperty(property)?.GetMaxLength() ?? fallback;

            var condition = _context.Model.FindEntityType(typeof(TraineeMedicalCondition))
                ?.FindProperty(nameof(TraineeMedicalCondition.Condition))?.GetMaxLength() ?? 200;

            return new TraineeFieldLimits(
                FirstName: Len(nameof(Trainee.FirstName), 50),
                LastName: Len(nameof(Trainee.LastName), 50),
                Ssn: Len(nameof(Trainee.SSN), 20),
                PhoneNumber: Len(nameof(Trainee.PhoneNumber), 20),
                ParentNumber: Len(nameof(Trainee.ParentNumber), 20),
                GuardianName: Len(nameof(Trainee.GuardianName), 50),
                Email: Owned(nameof(Trainee.Email), "Value", 200),
                Street: Owned(nameof(Trainee.Address), "Street", 70),
                City: Owned(nameof(Trainee.Address), "City", 50),
                MedicalCondition: condition);
        }
    }
}
