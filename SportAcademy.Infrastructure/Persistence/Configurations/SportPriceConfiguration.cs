using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class SportPriceConfiguration : IEntityTypeConfiguration<SportPrice>
    {
        public void Configure(EntityTypeBuilder<SportPrice> builder)
        {

            //Table Name
            builder.ToTable("SportPrices");

            //Pk
            // GroupType is part of the key: public and private training for the same
            // sport/branch/subscription type are priced separately, so each combination needs
            // its own row rather than one price covering both.
            builder.HasKey(sp => new { sp.SportId, sp.BranchId, sp.SubsTypeId, sp.GroupType });

            //props
            // Defaulted so the column can be added to a table that already has rows: existing
            // prices become the public price, and a private one is added per combination that
            // offers private training. The default must match SubscriptionDetails.GroupType's,
            // or the composite FK between them breaks for every pre-existing subscription.
            builder.Property(sp => sp.GroupType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(TraineeGroupType.Public);

            builder.Property(sp => sp.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            // Relationships
            // 1:M  Branch
            builder.HasOne(sp => sp.Branch)
                .WithMany(s => s.SportPrices)
                .HasForeignKey(sp => sp.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M  SportBranch
            builder.HasOne(sp => sp.SportBranch)
                .WithMany()
                .HasForeignKey(sp => new { sp.SportId, sp.BranchId })
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
                        sd.GroupType,
                    })
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
