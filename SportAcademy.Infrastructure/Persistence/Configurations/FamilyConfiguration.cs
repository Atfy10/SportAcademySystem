using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class FamilyConfiguration : IEntityTypeConfiguration<Family>
    {
        public void Configure(EntityTypeBuilder<Family> builder)
        {
            builder.ToTable("Families");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.FamilyCode)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(f => f.LastMemberNumber)
                .IsRequired()
                .HasColumnType("int");

            builder.HasMany(f => f.Members)
                .WithOne(t => t.Family)
                .HasForeignKey("FamilyId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
