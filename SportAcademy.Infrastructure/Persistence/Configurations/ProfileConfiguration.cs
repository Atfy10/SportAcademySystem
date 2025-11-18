using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class ProfileConfiguration : IEntityTypeConfiguration<Domain.Entities.Profile>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Profile> builder)
        {
            // Table Name
            builder.ToTable("Profiles");

            // PK
            builder.HasKey(p => p.AppUserId);

            // Props
            builder.Property(p => p.ProfileImageUrl)
                   .HasMaxLength(255); 

            builder.Property(p => p.Bio)
                   .HasMaxLength(300); 


            // Relationship
            // 1:1 AppUser
            builder.HasOne(p => p.User)
                   .WithOne(u => u.Profile)
                   .HasForeignKey<Profile>(p => p.AppUserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
