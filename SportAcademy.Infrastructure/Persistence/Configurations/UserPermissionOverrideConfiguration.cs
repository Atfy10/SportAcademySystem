using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
    {
        public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
        {
            builder.ToTable("UserPermissionOverrides");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Permission)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.Property(o => o.Effect)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(16);

            // One decision per (user, permission) - the admin API replaces the whole set for a
            // user rather than appending, so a duplicate row would only ever be a bug.
            builder.HasIndex(o => new { o.UserId, o.Permission }).IsUnique();

            builder.HasOne(o => o.User)
                   .WithMany()
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
