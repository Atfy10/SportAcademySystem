using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Configurations
{
    public class AppUserConfigurtion : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // TableName 
            builder.ToTable("AppUsers");

            builder.Property(u => u.IsBanned)
                .HasDefaultValue(false);

            //  Relations
            //  1:1  Emp
            builder.HasOne(u => u.Employee)
                .WithOne(e => e.AppUser)
                .HasForeignKey<Employee>(e => e.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:1  Trainee
            builder.HasOne(u => u.Trainee)
                .WithOne(t => t.AppUser)
                .HasForeignKey<Trainee>(t => t.AppUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:1 Profile
            builder.HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            //  1:M NotificationRecipients
            builder.HasMany(au => au.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
