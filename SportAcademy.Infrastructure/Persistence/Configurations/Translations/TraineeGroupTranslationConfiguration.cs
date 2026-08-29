using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Translations
{
    public class TraineeGroupTranslationConfiguration : IEntityTypeConfiguration<TraineeGroupTranslation>
    {
        public void Configure(EntityTypeBuilder<TraineeGroupTranslation> builder)
        {
            builder.ToTable("TraineeGroupTranslations");

            builder.HasKey(t => new { t.TraineeGroupId, t.LangCode });

            builder.Property(t => t.LangCode).IsRequired().HasMaxLength(5);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(150);

            builder.HasOne(t => t.TraineeGroup)
                .WithMany(g => g.Translations)
                .HasForeignKey(t => t.TraineeGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
