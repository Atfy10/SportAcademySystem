using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Configurations
{
    public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
        {
            builder.ToTable("NotificationRecipiens");

            builder.HasKey(x => new { x.UserId, x.NotificationId });

            builder.Property(x => x.IsRead)
                .HasDefaultValue(false);

            //  Relations
            //  M:1 User
            builder.HasOne(nr => nr.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //  M:1 Recipients
            builder.HasOne(nr => nr.Notification)
                .WithMany(n => n.Recipients)
                .HasForeignKey(n => n.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
