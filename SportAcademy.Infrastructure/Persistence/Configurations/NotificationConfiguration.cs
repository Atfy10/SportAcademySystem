using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notification");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Message)
                .IsRequired();

            builder.Property(x => x.GroupName)
                .IsRequired(false)
                .HasMaxLength(30);

            builder.Property(x => x.Title)
                .IsRequired(false)
                .HasMaxLength(100);

            var typeConverter = new ValueConverter<NotificationType?, string?>(
                v => v.HasValue ? v.Value.ToString() : null,
                v => string.IsNullOrEmpty(v) ? null : (NotificationType)Enum.Parse(typeof(NotificationType), v, true));

            builder.Property(x => x.Type)
                .IsRequired(false)
                .HasConversion(typeConverter)
                .HasDefaultValue(NotificationType.System);

            builder.Property(x => x.ActionUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();

            //  Relations
            builder.HasMany(n => n.Recipients)
                .WithOne(r => r.Notification)
                .HasForeignKey(r => r.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
