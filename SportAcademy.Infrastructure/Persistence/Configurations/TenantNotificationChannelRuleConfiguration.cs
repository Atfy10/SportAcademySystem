using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TenantNotificationChannelRuleConfiguration : IEntityTypeConfiguration<TenantNotificationChannelRule>
    {
        public void Configure(EntityTypeBuilder<TenantNotificationChannelRule> builder)
        {
            builder.ToTable("TenantNotificationChannelRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.IsEnabled)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(450);

            builder.HasIndex(x => new { x.TenantId, x.EventTypeId, x.Channel })
                .IsUnique()
                .HasDatabaseName("IX_TenantNotificationChannelRule_Tenant_EventType_Channel");

            builder.HasOne(x => x.EventType)
                .WithMany()
                .HasForeignKey(x => x.EventTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
