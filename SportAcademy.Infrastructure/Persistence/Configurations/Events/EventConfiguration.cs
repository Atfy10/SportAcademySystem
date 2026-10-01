using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Events;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Events
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.CancelReason).HasMaxLength(500);

            // KWD has 3 decimal places - same precision as every other money column.
            builder.Property(e => e.Price).HasPrecision(18, 3);
            builder.Property(e => e.DecorationFee).HasPrecision(18, 3);

            builder.Ignore(e => e.TotalPrice);

            builder.Property(e => e.RowVersion).IsRowVersion();

            // The list, the report and the overlap check all filter by branch + date range.
            builder.HasIndex(e => new { e.BranchId, e.StartsAt });

            builder.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EventCustomer)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.EventCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
