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
    public class SportSubscriptionTypeConfiguration : IEntityTypeConfiguration<SportSubscriptionType>
    {
        public void Configure(EntityTypeBuilder<SportSubscriptionType> builder)
        {
            //Table Name
            builder.ToTable("SportSubscriptionTypes");

            //PK
            builder.HasKey(sst => new { sst.SportId, sst.SubscriptionTypeId });

            // Relationships
            // 1:M  Sport
            builder.HasOne(sst => sst.Sport)
                .WithMany(s => s.SubscriptionTypes)
                .HasForeignKey(sp => sp.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M  SubscriptionType
            builder.HasOne(sst => sst.SubscriptionType)
                .WithMany(s => s.Sports)
                .HasForeignKey(sp => sp.SubscriptionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M SubscriptionDetails
            builder.HasMany(sst => sst.SubscriptionsDetails)
                .WithOne(sd => sd.SportSubscriptionType)
                .HasForeignKey(sd => new
                {
                    sd.SportSubscriptionTypeSportId,
                    sd.SportSubscriptionTypeSubscriptionTypeId,
                })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
