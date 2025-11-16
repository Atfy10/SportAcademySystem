using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class SportConfiguration : IEntityTypeConfiguration<Sport>
    {
        public void Configure(EntityTypeBuilder<Sport> builder)
        {
            // Table Name
            builder.ToTable("Sports");

            // PK
            builder.HasKey(s => s.Id);

            // Props
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Description)
                .HasMaxLength(500);

            builder.Property(s => s.Category)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(s => s.IsRequireHealthTest)
                .IsRequired()
                .HasDefaultValue(true);

            //  Relationships
            //  1:M  Coach
            builder.HasMany(s => s.Coaches)
                .WithOne(c => c.Sport)
                .HasForeignKey(c => c.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:M  SportSubscriptionType 
            builder.HasMany(s => s.SubscriptionTypes)
                .WithOne(sst => sst.Sport)
                .HasForeignKey(sst => sst.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:M  SportBranch 
            builder.HasMany(s => s.Branches)
                .WithOne(sb => sb.Sport)
                .HasForeignKey(sb => sb.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:M  SportTrainee 
            builder.HasMany(s => s.Trainees)
                .WithOne(st => st.Sport)
                .HasForeignKey(st => st.SportId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
