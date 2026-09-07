using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TenantImpersonationGrantConfiguration : IEntityTypeConfiguration<TenantImpersonationGrant>
    {
        public void Configure(EntityTypeBuilder<TenantImpersonationGrant> builder)
        {
            builder.ToTable("TenantImpersonationGrants");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(g => g.StartedAt).IsRequired();
            builder.Property(g => g.ExpiresAt).IsRequired();
            builder.Property(g => g.EndedReason).HasMaxLength(20);

            builder.HasOne(g => g.Tenant)
                .WithMany()
                .HasForeignKey(g => g.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(g => g.TenantId);
            builder.HasIndex(g => g.GrantedByUserId);
        }
    }
}
