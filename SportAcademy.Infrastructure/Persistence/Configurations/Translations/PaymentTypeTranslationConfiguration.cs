using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Translations
{
    public class PaymentTypeTranslationConfiguration : IEntityTypeConfiguration<PaymentTypeTranslation>
    {
        public void Configure(EntityTypeBuilder<PaymentTypeTranslation> builder)
        {
            builder.ToTable("PaymentTypeTranslations");

            builder.HasKey(t => new { t.PaymentTypeId, t.LangCode });

            builder.Property(t => t.LangCode).IsRequired().HasMaxLength(5);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(t => t.PaymentType)
                .WithMany(p => p.Translations)
                .HasForeignKey(t => t.PaymentTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
