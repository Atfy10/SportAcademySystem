using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Domain.Entities
{
    public class Trainee : Person
    {
        public int Id { get; set; }
        public TraineeCode TraineeCode { get; set; } = null!;
        public DateOnly JoinDate { get; set; }
        public bool IsSubscribed { get; set; }
        public string? ParentNumber { get; set; }
        public string? GuardianName { get; set; }
        public string? AppUserId { get; set; }
        public int BranchId { get; set; }
        public int FamilyId { get; set; }
        public int NationalityCategoryId { get; set; }

        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual AppUser? AppUser { get; set; }
        public virtual Family Family { get; set; } = null!;
        public virtual NationalityCategory NationalityCategory { get; set; } = null!;
        public virtual ICollection<TraineeCodesHistory> TraineeHistoryCode { get; set; } = [];
        public virtual ICollection<SportTrainee> Sports { get; set; } = [];
        public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
        public virtual ICollection<SubscriptionDetails> SubscriptionDetails { get; set; } = [];

        public AgeCategory AgeCategory
        {
            get
            {
                return GetAgeCategory();
            }
        }

        private AgeCategory GetAgeCategory()
        {
            var age = GetAge();

            if (age < 12) return AgeCategory.Kid;
            if (age < 18) return AgeCategory.Youth;
            return AgeCategory.Adult;
        }

        public int GetAge()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - BirthDate.Year;

            if (BirthDate > today.AddYears(-age))
                age--;
            return age;
        }
    }
}
