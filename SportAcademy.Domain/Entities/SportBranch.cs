namespace SportAcademy.Domain.Entities
{
    public class SportBranch
    {
        public int SportId { get; set; }
        public int BranchId { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
    }
}
