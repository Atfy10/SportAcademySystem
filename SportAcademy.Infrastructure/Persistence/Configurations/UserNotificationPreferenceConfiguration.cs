using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
    {
        public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
        {
            builder.ToTable("UserNotificationPreferences");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.IsEnabled)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.HasIndex(x => new { x.UserId, x.Channel })
                .IsUnique()
                .HasDatabaseName("IX_UserNotificationPreference_User_Channel");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
