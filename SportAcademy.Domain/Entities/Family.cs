namespace SportAcademy.Domain.Entities
{
    public class Family
    {
        public int Id { get; set; }

        public int FamilyCode { get; set; }

        public int LastMemberNumber { get; set; }

        public ICollection<Trainee> Members { get; } = [];
    }
}
