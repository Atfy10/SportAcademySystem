using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.EmployeeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views
{
    public class EmployeeBasicViewConfiguration
    : IEntityTypeConfiguration<EmployeeBasicView>
    {
        public void Configure(EntityTypeBuilder<EmployeeBasicView> builder)
        {
            builder.ToView("vw_EmployeeBasic");

            builder.HasNoKey();

            builder.Property(x => x.Id);
            builder.Property(x => x.FirstName);
            builder.Property(x => x.LastName);
            builder.Property(x => x.SSN);
            builder.Property(x => x.Email);
            builder.Property(x => x.BirthDate);
            builder.Property(x => x.Gender);
            builder.Property(x => x.City);
            builder.Property(x => x.SecondPhoneNumber);
        }
    }
}