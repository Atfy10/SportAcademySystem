using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(i => i.Purpose)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Property(i => i.Role)
            .HasMaxLength(64);

        // Comma-separated permission keys - simple scalar column, avoids a join table for
        // a handful of strings that are only ever read/written as a whole set.
        builder.Property(i => i.Permissions)
            .HasMaxLength(2000);

        builder.HasIndex(i => i.TokenHash)
            .IsUnique();

        builder.HasIndex(i => new { i.TenantId, i.Email, i.Status });

        builder.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
