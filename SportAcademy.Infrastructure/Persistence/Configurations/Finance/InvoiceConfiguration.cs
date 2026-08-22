using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(i => i.Id);

            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
            builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
            builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(i => i.Notes).HasMaxLength(1000);

            // KWD (and several other Gulf currencies) has 3 decimal places - decimal(18,2)
            // would silently truncate fils.
            builder.Property(i => i.SubTotal).HasPrecision(18, 3);
            builder.Property(i => i.DiscountTotal).HasPrecision(18, 3);
            builder.Property(i => i.TaxTotal).HasPrecision(18, 3);
            builder.Property(i => i.GrandTotal).HasPrecision(18, 3);
            builder.Property(i => i.AmountPaid).HasPrecision(18, 3);

            builder.Ignore(i => i.Outstanding);

            builder.HasOne(i => i.Trainee)
                   .WithMany()
                   .HasForeignKey(i => i.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Branch)
                   .WithMany(b => b.Invoices)
                   .HasForeignKey(i => i.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Lines)
                   .WithOne(l => l.Invoice)
                   .HasForeignKey(l => l.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(i => i.Allocations)
                   .WithOne(a => a.Invoice)
                   .HasForeignKey(a => a.InvoiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
