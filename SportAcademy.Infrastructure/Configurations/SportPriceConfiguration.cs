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
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();


            // Relationships

            // 1:M  Sport
            builder.HasOne(sp => sp.Sport)
                   .WithMany(s => s.Prices)
                   .HasForeignKey(sp => sp.SportId)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:M  Branch
            builder.HasOne(sp => sp.Branch)
                   .WithMany(s => s.SportPrices)
                   .HasForeignKey(sp => sp.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);
            // 1:M  SubscriptionType
            builder.HasOne(sp => sp.SubscriptionType)
                    .WithMany(st => st.SportPrices)
                    .HasForeignKey(sp => sp.SubsTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
