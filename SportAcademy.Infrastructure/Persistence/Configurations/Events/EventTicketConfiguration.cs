using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Events;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Events
{
    public class EventTicketConfiguration : IEntityTypeConfiguration<EventTicket>
    {
        public void Configure(EntityTypeBuilder<EventTicket> builder)
        {
            builder.ToTable("EventTickets");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token).IsRequired().HasMaxLength(64);
            // The guest's ticket page looks a ticket up by this with no tenant context - it must
            // be unique across every academy, not just within one.
            builder.HasIndex(t => t.Token).IsUnique();

            // Ticket #1..#Capacity of one event, each number once.
            builder.HasIndex(t => new { t.EventId, t.Number }).IsUnique();

            builder.Property(t => t.GuestName).HasMaxLength(100);
            builder.Property(t => t.RowVersion).IsRowVersion();

            builder.Ignore(t => t.IsAdmitted);

            builder.HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
