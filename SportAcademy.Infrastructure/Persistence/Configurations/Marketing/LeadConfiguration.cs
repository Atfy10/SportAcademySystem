using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Marketing;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Marketing;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.FullName).IsRequired().HasMaxLength(150);
        builder.Property(l => l.AcademyName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Email).IsRequired().HasMaxLength(256);
        builder.Property(l => l.PhoneNumber).IsRequired().HasMaxLength(30);
        builder.Property(l => l.City).HasMaxLength(100);
        builder.Property(l => l.Message).HasMaxLength(2000);

        builder.Property(l => l.Locale).IsRequired().HasMaxLength(5);
        builder.Property(l => l.SourcePage).HasMaxLength(300);
        builder.Property(l => l.UtmSource).HasMaxLength(150);
        builder.Property(l => l.UtmMedium).HasMaxLength(150);
        builder.Property(l => l.UtmCampaign).HasMaxLength(150);
        builder.Property(l => l.Referrer).HasMaxLength(500);
        builder.Property(l => l.IpHash).HasMaxLength(64);

        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.InternalNotes).HasMaxLength(4000);

        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasIndex(l => l.CreatedAt);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.Email);
    }
}
