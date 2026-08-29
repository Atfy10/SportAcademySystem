using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Translations
{
    public class NationalityCategoryTranslationConfiguration : IEntityTypeConfiguration<NationalityCategoryTranslation>
    {
        public void Configure(EntityTypeBuilder<NationalityCategoryTranslation> builder)
        {
            builder.ToTable("NationalityCategoryTranslations");

            builder.HasKey(t => new { t.NationalityCategoryId, t.LangCode });

            builder.Property(t => t.LangCode).IsRequired().HasMaxLength(5);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(t => t.NationalityCategory)
                .WithMany(n => n.Translations)
                .HasForeignKey(t => t.NationalityCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
