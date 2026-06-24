using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

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

            builder.HasData(
                new SubscriptionType
                {
                    Id = 1,
                    Name = SubType.Basic,
                    DaysPerMonth = 8,
                    NumberOfMonths = 1,
                    IsActive = true,
                    IsOffer = false
                },
                new SubscriptionType
                {
                    Id = 2,
                    Name = SubType.Silver,
                    DaysPerMonth = 12,
                    NumberOfMonths = 1,
                    IsActive = true,
                    IsOffer = false
                },
                new SubscriptionType
                {
                    Id = 3,
                    Name = SubType.Gold,
                    DaysPerMonth = 16,
                    NumberOfMonths = 1,
                    IsActive = true,
                    IsOffer = false
                },
                new SubscriptionType
                {
                    Id = 4,
                    Name = SubType.Platinum,
                    DaysPerMonth = 24,
                    NumberOfMonths = 1,
                    IsActive = true,
                    IsOffer = true
                },
                new SubscriptionType
                {
                    Id = 5,
                    Name = SubType.Monthly,
                    DaysPerMonth = 20,
                    NumberOfMonths = 1,
                    IsActive = true,
                    IsOffer = false
                }
            );
        }
    }
}
