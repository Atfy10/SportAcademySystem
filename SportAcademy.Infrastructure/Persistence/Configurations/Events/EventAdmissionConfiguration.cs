using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Events;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Events
{
    public class EventAdmissionConfiguration : IEntityTypeConfiguration<EventAdmission>
    {
        public void Configure(EntityTypeBuilder<EventAdmission> builder)
        {
            builder.ToTable("EventAdmissions");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.DeviceKey).IsRequired().HasMaxLength(64);

            // A phone is let in once per event: a second scan from it finds this row instead of
            // taking another place, even if two scans from it race.
            builder.HasIndex(a => new { a.EventId, a.DeviceKey }).IsUnique();

            builder.HasOne(a => a.Event)
                .WithMany(e => e.Admissions)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
