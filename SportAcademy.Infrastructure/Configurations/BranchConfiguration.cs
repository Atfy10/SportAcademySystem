using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            // Table name
            builder.ToTable("Branches");

            // Pk
            builder.HasKey(b => b.Id);

            // Props
            builder.Property(b => b.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.City)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.Country)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(b => b.Email)
                   .HasMaxLength(100);

            builder.Property(b => b.CoX)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(b => b.CoY)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(b => b.IsActive)
                   .IsRequired();

            // Relationships:

            // 1:M Sessions
            builder.HasMany(b => b.Sessions)
                   .WithOne(s => s.Branch)
                   .HasForeignKey(s => s.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:M Employees
            builder.HasMany(b => b.Employees)
                   .WithOne(e => e.Branch)
                   .HasForeignKey(e => e.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:M Payments
            builder.HasMany(b => b.Payments)
                   .WithOne(p => p.Branch)
                   .HasForeignKey(p => p.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:M SportBranch 
            builder.HasMany(b => b.Sports)
                   .WithOne(sb => sb.Branch)
                   .HasForeignKey(sb => sb.BranchId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1:M SportPrice 
            builder.HasMany(b => b.SportPrices)
                   .WithOne(sp => sp.Branch)
                   .HasForeignKey(sp => sp.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
