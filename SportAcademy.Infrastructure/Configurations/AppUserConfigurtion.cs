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
            //PK
            builder.HasKey(a => a.Id);

            // Relationships

            // 1:1  Emp
            builder.HasOne(u => u.Employee)
                   .WithOne(e => e.AppUser)
                   .HasForeignKey<Employee>(e => e.AppUserId)
                   .OnDelete(DeleteBehavior.Restrict);



            // 1:1  Trainee
            builder.HasOne(u => u.Trainee)
                .WithOne(t => t.AppUser)
                .HasForeignKey<Trainee>(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
