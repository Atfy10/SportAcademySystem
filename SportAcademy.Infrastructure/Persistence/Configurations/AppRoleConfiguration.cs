using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            builder.HasData(
                new AppRole
                {
                    Id = Guid.Parse("00000001-0000-0000-0000-000000000001"),
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    ConcurrencyStamp = ""
                },
                new AppRole
                {
                    Id = Guid.Parse("00000002-0000-0000-0000-000000000002"),
                    Name = "Owner",
                    NormalizedName = "OWNER",
                    ConcurrencyStamp = ""
                },
                new AppRole
                {
                    Id = Guid.Parse("00000003-0000-0000-0000-000000000003"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = ""
                },
                new AppRole
                {
                    Id = Guid.Parse("00000004-0000-0000-0000-000000000004"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = ""
                },
                new AppRole
                {
                    Id = Guid.Parse("00000005-0000-0000-0000-000000000005"),
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = ""
                }
            );
        }
    }
}
