using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public class NotificationGroupMemberConfiguration : IEntityTypeConfiguration<NotificationGroupMember>
{
    public void Configure(EntityTypeBuilder<NotificationGroupMember> builder)
    {
        builder.ToTable("NotificationGroupMembers");

        builder.HasKey(m => new { m.UserId, m.GroupName });

        builder.Property(m => m.GroupName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(m => m.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
