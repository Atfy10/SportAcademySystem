using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class TraineeBasicViewConfiguration
    : IEntityTypeConfiguration<TraineeBasicView>
{
    public void Configure(EntityTypeBuilder<TraineeBasicView> builder)
    {
        builder.ToView("vw_TraineeBasic");

        builder.HasNoKey();

        builder.Property(x => x.Id);

        builder.Property(x => x.FirstName);

        builder.Property(x => x.LastName);

        builder.Property(x => x.SSN);

        builder.Property(x => x.Email);

        builder.Property(x => x.BirthDate);

        builder.Property(x => x.Gender)
               .HasConversion<string>();

        builder.Property(x => x.City);

        builder.Property(x => x.SecondPhoneNumber);
    }
}