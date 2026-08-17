using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public class TraineeMedicalConditionConfiguration : IEntityTypeConfiguration<TraineeMedicalCondition>
{
    public void Configure(EntityTypeBuilder<TraineeMedicalCondition> builder)
    {
        builder.ToTable("TraineeMedicalConditions");

        builder.HasKey(tmc => tmc.Id);

        builder.Property(tmc => tmc.Condition)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(tmc => tmc.Trainee)
            .WithMany(t => t.MedicalConditions)
            .HasForeignKey(tmc => tmc.TraineeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
