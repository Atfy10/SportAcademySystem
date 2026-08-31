using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Translations
{
    public class FamilyTranslationConfiguration : IEntityTypeConfiguration<FamilyTranslation>
    {
        public void Configure(EntityTypeBuilder<FamilyTranslation> builder)
        {
            builder.ToTable("FamilyTranslations");

            builder.HasKey(t => new { t.FamilyId, t.LangCode });

            builder.Property(t => t.LangCode).IsRequired().HasMaxLength(5);
            builder.Property(t => t.Name).HasMaxLength(200);
            builder.Property(t => t.GuardianName).HasMaxLength(200);

            builder.HasOne(t => t.Family)
                .WithMany(f => f.Translations)
                .HasForeignKey(t => t.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
