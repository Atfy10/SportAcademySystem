using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TraineeCodesHistoryConfiguration : IEntityTypeConfiguration<TraineeCodesHistory>
    {
        public void Configure(EntityTypeBuilder<TraineeCodesHistory> builder)
        {
            builder.ToTable("TraineeCodesHistory");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.OldTraineeCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.ChangedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            builder.Property(t => t.Reason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasOne(tch => tch.Trainee)
                .WithMany(t => t.TraineeHistoryCode)
                .HasForeignKey(t => t.TraineeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
