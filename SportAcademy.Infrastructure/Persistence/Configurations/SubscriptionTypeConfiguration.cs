using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class SubscriptionTypeConfiguration : IEntityTypeConfiguration<SubscriptionType>
    {
        public void Configure(EntityTypeBuilder<SubscriptionType> builder)
        {
            //Table Name
            builder.ToTable("SubscriptionTypes");

            // PK
            builder.HasKey(st => st.Id);

            // Props
            builder.Property(st => st.Name)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(st => st.DaysPerMonth)
                .IsRequired();

            builder.Property(st => st.IsActive)
                .HasDefaultValue(true);

            builder.Property(st => st.IsOffer)
                .HasDefaultValue(false);

            // Relationships

            // 1:M SportSubscriptionTypes
            builder.HasMany(st => st.Sports)
                .WithOne(sst => sst.SubscriptionType)
                .HasForeignKey(sst => sst.SubscriptionTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
