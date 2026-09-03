using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class UserBranchAccessConfiguration : IEntityTypeConfiguration<UserBranchAccess>
    {
        public void Configure(EntityTypeBuilder<UserBranchAccess> builder)
        {
            builder.ToTable("UserBranchAccesses");

            builder.HasKey(a => a.Id);

            // One decision per (user, branch) - the admin API replaces the whole set for a user
            // rather than appending, so a duplicate row would only ever be a bug.
            builder.HasIndex(a => new { a.UserId, a.BranchId }).IsUnique();

            builder.HasOne(a => a.User)
                   .WithMany()
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Branch)
                   .WithMany()
                   .HasForeignKey(a => a.BranchId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
