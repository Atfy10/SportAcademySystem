using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class CoachBranchAccessConfiguration : IEntityTypeConfiguration<CoachBranchAccess>
    {
        public void Configure(EntityTypeBuilder<CoachBranchAccess> builder)
        {
            builder.ToTable("CoachBranchAccesses");

            builder.HasKey(a => a.Id);

            // One decision per (coach, branch) - the admin API replaces the whole set for a
            // coach rather than appending, so a duplicate row would only ever be a bug.
            builder.HasIndex(a => new { a.CoachId, a.BranchId }).IsUnique();

            builder.HasOne(a => a.Coach)
                   .WithMany()
                   .HasForeignKey(a => a.CoachId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Branch)
                   .WithMany()
                   .HasForeignKey(a => a.BranchId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
