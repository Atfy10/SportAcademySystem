namespace SportAcademy.Domain.Entities
{
    public class Family
    {
        public int Id { get; private set; }

        public int FamilyCode { get; private set; }

        public int LastMemberNumber { get; private set; }

        public ICollection<Trainee> Members { get; } = [];
    }
}
