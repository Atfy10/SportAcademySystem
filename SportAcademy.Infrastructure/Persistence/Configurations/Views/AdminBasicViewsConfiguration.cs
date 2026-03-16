using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.AdminViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views
{
    public class AdminBasicViewsConfiguration : IEntityTypeConfiguration<AdminBasicViews>
    {
        public void Configure(EntityTypeBuilder<AdminBasicViews> builder)
        {
            builder.ToView("vw_AdminBasic");

            builder.HasNoKey();

            builder.Property(x => x.UserName)
                .HasColumnName("UserName")
                .HasMaxLength(50);

            builder.Property(x => x.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100);

            builder.Property(x => x.SSN)
                .HasColumnName("SSN")
                .HasMaxLength(14);

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .HasMaxLength(256);

            builder.Property(x => x.BirthDate)
                .HasColumnName("BirthDate");

            builder.Property(x => x.Gender)
                .HasColumnName("Gender");

            builder.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100);

            builder.Property(x => x.SecondPhoneNumber)
                .HasColumnName("SecondPhoneNumber")
                .HasMaxLength(20);

            builder.Property(x => x.ProfileImageUrl)
                .HasColumnName("ProfileImageUrl");

            builder.Property(x => x.Bio)
                .HasColumnName("Bio");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");
        }
    }
}