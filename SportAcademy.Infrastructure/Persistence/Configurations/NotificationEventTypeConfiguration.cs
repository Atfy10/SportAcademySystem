using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class NotificationEventTypeConfiguration : IEntityTypeConfiguration<NotificationEventType>
    {
        public void Configure(EntityTypeBuilder<NotificationEventType> builder)
        {
            builder.ToTable("NotificationEventTypes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Key)
                .IsUnique()
                .HasDatabaseName("IX_NotificationEventType_Key");

            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.DefaultStyle)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.CreatedAt)
                .IsRequired();
        }
    }
}
