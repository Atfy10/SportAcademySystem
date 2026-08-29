using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Translations
{
    public class BranchTranslationConfiguration : IEntityTypeConfiguration<BranchTranslation>
    {
        public void Configure(EntityTypeBuilder<BranchTranslation> builder)
        {
            builder.ToTable("BranchTranslations");

            builder.HasKey(t => new { t.BranchId, t.LangCode });

            builder.Property(t => t.LangCode).IsRequired().HasMaxLength(5);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
            builder.Property(t => t.City).HasMaxLength(100);
            builder.Property(t => t.Country).HasMaxLength(100);

            builder.HasOne(t => t.Branch)
                .WithMany(b => b.Translations)
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
