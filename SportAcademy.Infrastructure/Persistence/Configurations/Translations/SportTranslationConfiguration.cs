using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Translations
{
    public class SportTranslationConfiguration : IEntityTypeConfiguration<SportTranslation>
    {
        public void Configure(EntityTypeBuilder<SportTranslation> builder)
        {
            builder.ToTable("SportTranslations");

            builder.HasKey(t => new { t.SportId, t.LangCode });

            builder.Property(t => t.LangCode).IsRequired().HasMaxLength(5);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Description).HasMaxLength(500);

            builder.HasOne(t => t.Sport)
                .WithMany(s => s.Translations)
                .HasForeignKey(t => t.SportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
