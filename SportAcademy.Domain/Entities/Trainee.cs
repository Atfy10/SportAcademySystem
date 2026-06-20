using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Domain.Entities
{
    public class Trainee : Person
    {
        private List<SportTrainee> _sports = [];
        private List<Enrollment> _enrollments = [];
        private List<SubscriptionDetails> _subscriptionDetails = [];
        private List<TraineeCodesHistory> _traineeHistoryCode = [];

        private Trainee(
            PersonData data,
            string? parentNumber,
            string? guardianName,
            int branchId,
            int nationalityCategoryId)
            : base(data)
        {
            JoinDate = DateOnly.FromDateTime(DateTime.UtcNow);
            IsSubscribed = false;
            ParentNumber = parentNumber;
            GuardianName = guardianName;
            BranchId = branchId;
            NationalityCategoryId = nationalityCategoryId;
            _sports = [];
        }

        private Trainee() { }

        public int Id { get; private set; }
        public TraineeCode TraineeCode { get; private set; } = null!;
        public DateOnly JoinDate { get; private set; }
        public bool IsSubscribed { get; private set; }
        public string? ParentNumber { get; private set; }
        public string? GuardianName { get; private set; }
        public string? AppUserId { get; private set; }
        public int BranchId { get; private set; }
        public int FamilyId { get; private set; }
        public int NationalityCategoryId { get; private set; }

        public AgeCategory AgeCategory => GetAgeCategory();

        public IReadOnlyCollection<SportTrainee> Sports => _sports.AsReadOnly();
        public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();
        public IReadOnlyCollection<SubscriptionDetails> SubscriptionDetails => _subscriptionDetails.AsReadOnly();
        public IReadOnlyCollection<TraineeCodesHistory> TraineeHistoryCode => _traineeHistoryCode.AsReadOnly();

        public virtual Branch Branch { get; set; } = null!;
        public virtual AppUser? AppUser { get; set; }
        public virtual Family Family { get; set; } = null!;
        public virtual NationalityCategory NationalityCategory { get; set; } = null!;

        public static Trainee Create(
            PersonData data,
            string? parentNumber,
            string? guardianName,
            int branchId,
            int nationalityCategoryId)
        {
            if (!IsSsnValid(data.SSN, data.BirthDate))
                throw new Domain.Exceptions.SharedExceptions.SSNSyntaxErrorException();

            var ageCategory = GetAgeCategory(data.BirthDate);
            bool isAdult = ageCategory == AgeCategory.Adult;
            bool isGuardianInfoMissing = string.IsNullOrWhiteSpace(parentNumber)
                || string.IsNullOrWhiteSpace(guardianName);
            if (!isAdult && isGuardianInfoMissing)
                throw new GuardianInfoMissingException();

            return new Trainee(data, parentNumber, guardianName, branchId, nationalityCategoryId);
        }

        public void AssignSport(int sportId, SkillLevel skillLevel = SkillLevel.NotSpecified)
        {
            if (_sports.Any(s => s.SportId == sportId))
                return;
            _sports.Add(new SportTrainee { SportId = sportId, SkillLevel = skillLevel });
        }

        public void RemoveSport(int sportId)
        {
            var sport = _sports.FirstOrDefault(s => s.SportId == sportId);
            if (sport != null)
                _sports.Remove(sport);
        }

        public void SetTraineeCode(TraineeCode code)
        {
            TraineeCode = code;
        }

        public void SetIds(int id, int familyId)
        {
            Id = id;
            FamilyId = familyId;
        }

        public void AssignUser(string appUserId)
        {
            AppUserId = appUserId;
        }

        private AgeCategory GetAgeCategory()
        {
            var age = CalculateAge();
            if (age < 12) return AgeCategory.Kid;
            if (age < 18) return AgeCategory.Youth;
            return AgeCategory.Adult;
        }

        private static AgeCategory GetAgeCategory(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age))
                age--;
            if (age < 12) return AgeCategory.Kid;
            if (age < 18) return AgeCategory.Youth;
            return AgeCategory.Adult;
        }
    }
}
