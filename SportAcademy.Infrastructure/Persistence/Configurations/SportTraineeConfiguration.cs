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
    public class SportTraineeConfiguration : IEntityTypeConfiguration<SportTrainee>
    {
        public void Configure(EntityTypeBuilder<SportTrainee> builder)
        {
            //Table Name
            builder.ToTable("SportTrainees");
            //PK
            builder.HasKey(st => new { st.SportId, st.TraineeId });

            //props
            builder.Property(st => st.SkillLevel)
                  .IsRequired()
                  .HasConversion<string>();

            // Relationships

            // 1:M  Sport
            builder.HasOne(st => st.Sport)
                .WithMany(s => s.Trainees)
                .HasForeignKey(st => st.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M  Trainee
            builder.HasOne(st => st.Trainee)
                .WithMany(t => t.Sports)
                .HasForeignKey(st => st.TraineeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
