using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.ValueObjects
{
    public sealed class TraineeCode : ValueObject
    {
        public string Value { get; }

        public AgeCategory Age { get; }
        public int FamilyCode { get; }
        public int BranchId { get; }
        public string NationalityCode { get; }
        public int MemberNumber { get; }

        private TraineeCode(
            AgeCategory age,
            int familyCode,
            int branchId,
            string nationalityCode,
            int memberNumber)
        {
            Age = age;
            FamilyCode = familyCode;
            BranchId = branchId;
            NationalityCode = nationalityCode;
            MemberNumber = memberNumber;

            Value = $"{(char)age}-{familyCode}-{branchId}-{nationalityCode}-{memberNumber:D4}";
        }

        private TraineeCode(
            string value,
            AgeCategory age,
            int familyCode,
            int branchId,
            string nationalityCode,
            int memberNumber)
        {
            Value = value;
            Age = age;
            FamilyCode = familyCode;
            BranchId = branchId;
            NationalityCode = nationalityCode;
            MemberNumber = memberNumber;
        }

        public static TraineeCode Create(
            AgeCategory age,
            int familyCode,
            int branchId,
            string nationalityCode,
            int memberNumber)
        {
            if (branchId < 0 || branchId > 999)
                throw new ArgumentOutOfRangeException(nameof(branchId));

            if (memberNumber < 0 || memberNumber > 9999)
                throw new ArgumentOutOfRangeException(nameof(memberNumber));

            return new TraineeCode(
                age,
                familyCode,
                branchId,
                nationalityCode,
                memberNumber);
        }

        public static TraineeCode FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid trainee code");

            ReadOnlySpan<char> span = value.AsSpan();

            int firstDash = span.IndexOf('-');
            int secondDash = span.Slice(firstDash + 1).IndexOf('-') + firstDash + 1;
            int thirdDash = span.Slice(secondDash + 1).IndexOf('-') + secondDash + 1;
            int fourthDash = span.Slice(thirdDash + 1).IndexOf('-') + thirdDash + 1;

            var age = (AgeCategory)span[0];

            int familyCode = int.Parse(span.Slice(firstDash + 1, secondDash - firstDash - 1));

            int branchId = int.Parse(span.Slice(secondDash + 1, thirdDash - secondDash - 1));

            string nationality = span
                .Slice(thirdDash + 1, fourthDash - thirdDash - 1)
                .ToString();

            int memberNumber = int.Parse(span.Slice(fourthDash + 1));

            return new TraineeCode(
                value,
                age,
                familyCode,
                branchId,
                nationality,
                memberNumber);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
