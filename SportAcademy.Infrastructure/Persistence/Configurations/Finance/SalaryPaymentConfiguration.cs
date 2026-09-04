using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class SalaryPaymentConfiguration : IEntityTypeConfiguration<SalaryPayment>
    {
        public void Configure(EntityTypeBuilder<SalaryPayment> builder)
        {
            builder.ToTable("SalaryPayments");
            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(sp => sp.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(sp => sp.Notes)
                .HasMaxLength(1000);

            builder.Property(sp => sp.RejectionReason)
                .HasMaxLength(500);

            // KWD (and several other Gulf currencies) has 3 decimal places - decimal(18,2)
            // would silently truncate fils.
            builder.Property(sp => sp.Amount).HasPrecision(18, 3);

            builder.HasOne(sp => sp.Employee)
                .WithMany()
                .HasForeignKey(sp => sp.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sp => sp.Branch)
                .WithMany()
                .HasForeignKey(sp => sp.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sp => sp.PaymentType)
                .WithMany()
                .HasForeignKey(sp => sp.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
