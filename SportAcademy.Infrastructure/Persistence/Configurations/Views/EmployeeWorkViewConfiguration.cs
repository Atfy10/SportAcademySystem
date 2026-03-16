using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.EmployeeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views
{
    public class EmployeeWorkViewConfiguration
    : IEntityTypeConfiguration<EmployeeWorkView>
    {
        public void Configure(EntityTypeBuilder<EmployeeWorkView> builder)
        {
            builder.ToView("vw_EmployeeWork");

            builder.HasNoKey();

            builder.Property(x => x.Id);
            builder.Property(x => x.Salary);
            builder.Property(x => x.HireDate);
            builder.Property(x => x.Position);
            builder.Property(x => x.BranchName);
            builder.Property(x => x.UserName);
        }
    }
}