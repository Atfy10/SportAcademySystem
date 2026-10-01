using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Events;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Events
{
    public class EventCustomerConfiguration : IEntityTypeConfiguration<EventCustomer>
    {
        public void Configure(EntityTypeBuilder<EventCustomer> builder)
        {
            builder.ToTable("EventCustomers");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(c => c.Notes).HasMaxLength(1000);

            // The phone number is how a returning customer is found, so it must identify exactly
            // one live customer per academy. Filtered on IsDeleted so a deleted customer's number
            // can be registered again.
            builder.HasIndex(c => new { c.TenantId, c.PhoneNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => c.FullName);

            builder.HasOne(c => c.NationalityCategory)
                .WithMany()
                .HasForeignKey(c => c.NationalityCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
