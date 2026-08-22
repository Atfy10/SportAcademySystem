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

            builder.Property(f => f.Id)
                .HasDefaultValueSql("NEXT VALUE FOR FamilyCodeSequence");

            builder.Property(f => f.FamilyCode)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(f => f.LastMemberNumber)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(f => f.Name)
                .HasMaxLength(200);

            builder.Property(f => f.GuardianName)
                .HasMaxLength(200);

            builder.Property(f => f.GuardianPhone)
                .HasMaxLength(30);

            builder.HasMany(f => f.Members)
                .WithOne(t => t.Family)
                .HasForeignKey("FamilyId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
