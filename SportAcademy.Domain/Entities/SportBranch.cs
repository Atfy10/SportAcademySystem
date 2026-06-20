namespace SportAcademy.Domain.Entities
{
    public class SportBranch
    {
        private SportBranch() { }

        private SportBranch(int sportId, int branchId)
        {
            SportId = sportId;
            BranchId = branchId;
        }

        public int SportId { get; private set; }
        public int BranchId { get; private set; }

        public virtual Sport Sport { get; private set; } = null!;
        public virtual Branch Branch { get; private set; } = null!;

        public static SportBranch Create(int sportId, int branchId)
        {
            return new SportBranch(sportId, branchId);
        }
    }
}
