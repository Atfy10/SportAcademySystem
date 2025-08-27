using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Configurations
{
    public class SportBranchConfiguration : IEntityTypeConfiguration<SportBranch>
    {
        public void Configure(EntityTypeBuilder<SportBranch> builder)
        {
            // Table Name
            builder.ToTable("SportBranches");

            // PK
            builder.HasKey(sb => new { sb.SportId, sb.BranchId });

            // 1:M Sport
            builder.HasOne(sb => sb.Sport)
                   .WithMany(s => s.Branches)
                   .HasForeignKey(sb => sb.SportId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:M Branch
            builder.HasOne(sb => sb.Branch)
                   .WithMany(b => b.Sports)
                   .HasForeignKey(sb => sb.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
