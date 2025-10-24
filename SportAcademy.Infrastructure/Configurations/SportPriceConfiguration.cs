using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Configurations
{
    public class SportPriceConfiguration : IEntityTypeConfiguration<SportPrice>
    {
        public void Configure(EntityTypeBuilder<SportPrice> builder)
        {

            //Table Name
            builder.ToTable("SportPrices");
            //Pk 
            builder.HasKey(sp => new { sp.SportId, sp.BranchId, sp.SubsTypeId });

            //props
            builder.Property(sp => sp.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            // Relationships
            // 1:M  Branch
            builder.HasOne(sp => sp.Branch)
                .WithMany(s => s.SportPrices)
                .HasForeignKey(sp => sp.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

                // 1:M  SportSubscriptionType
                builder.HasOne(sp => sp.SportSubscriptionType)
                    .WithMany(st => st.SportPrices)
                    .HasForeignKey(sp => new
                    {
                        sp.SportId,
                        sp.SubsTypeId
                    }).OnDelete(DeleteBehavior.Cascade);

                // 1:M  SubscriptionDetails
                builder.HasMany(sp => sp.SubscriptionsDetails)
                    .WithOne(sd => sd.SportPrice)
                    .HasForeignKey(sd => new
                    {
                        sd.SportId,
                        sd.BranchId,
                        sd.SubscriptionTypeId,
                    })
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
