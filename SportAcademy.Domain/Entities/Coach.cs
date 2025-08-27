namespace SportAcademy.Domain.Entities
{
    public class Coach
    {
        public required string SkillLevel { get; set; }
        public int EmployeeId { get; set; }
        public int SportId { get; set; }
        public DateOnly BirthDate { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Sport Sport { get; set; } = null!;
        //Nav prop
        public virtual ICollection<Session> Sessions { get; set; } = [];
    }
}
