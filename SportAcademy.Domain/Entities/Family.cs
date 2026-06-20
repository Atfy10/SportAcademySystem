namespace SportAcademy.Domain.Entities
{
    public class Family
    {
        private List<Trainee> _members = [];

        private Family() { }

        public int Id { get; private set; }
        public int FamilyCode { get; private set; }
        public int LastMemberNumber { get; private set; }
        public IReadOnlyCollection<Trainee> Members => _members.AsReadOnly();

        public static Family Create()
        {
            return new Family
            {
                LastMemberNumber = 0
            };
        }

        public int GenerateNextMemberNumber()
        {
            LastMemberNumber++;
            return LastMemberNumber;
        }

        public void SetFamilyCode(int code)
        {
            FamilyCode = code;
        }
    }
}
