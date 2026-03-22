using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings.EnrollmentProfile
{
    public class EnrollmentMappingProfile : AutoMapper.Profile
    {
        public EnrollmentMappingProfile()
        {
            CreateMap<Enrollment, EnrollmentDto>()
                .ReverseMap();

            CreateMap<CreateEnrollmentCommand, Enrollment>();

            CreateMap<UpdateEnrollmentCommand, Enrollment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Enrollment, EnrollmentDataDto>()
                .ConstructUsing(src => new EnrollmentDataDto(
                    src.Id,
                    src.EnrollmentDate,
                    src.ExpiryDate,
                    src.SessionAllowed,
                    src.SessionRemaining,
                    src.IsActive,
                    src.Trainee.FirstName + " " + src.Trainee.LastName,
                    src.TraineeGroup.Coach.Employee.FirstName + " " + src.TraineeGroup.Coach.Employee.LastName,
                    src.SubscriptionDetailsId
                ));

            CreateMap<Enrollment, EnrollmentCardDto>()
                .ConstructUsing(src => new EnrollmentCardDto(
                    src.Id,
                    src.Trainee.FirstName + " " + src.Trainee.LastName,
                    src.Trainee.AppUser.Email ?? "",
                    src.TraineeGroup!.Coach!.Sport!.Name,
                    src.SubscriptionDetails.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString(),
                    src.TraineeGroup.Branch!.Name,
                    src.TraineeGroup.Coach.Employee.FirstName + " " + src.TraineeGroup.Coach.Employee.LastName,
                    src.EnrollmentDate.ToString("yyyy-MM-dd"),
                    src.SubscriptionDetails.StartDate.ToString("yyyy-MM-dd"),
                    src.SubscriptionDetails.EndDate.ToString("yyyy-MM-dd"),
                    src.SubscriptionDetails.SportPrice.Price,
                    src.GetPaymentStatus(),
                    src.GetStatus(),
                    GetSessionsCompleted(src.Attendances),
                    src.SessionAllowed
                ));

            CreateMap<Enrollment, EnrollmentDetailDto>()
                .ConstructUsing(src => new EnrollmentDetailDto(
                    src.Id,
                    src.Trainee.FirstName + " " + src.Trainee.LastName,
                    src.Trainee.AppUser.Email ?? "",
                    src.TraineeGroup!.Coach!.Sport!.Name,
                    src.SubscriptionDetails.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString(),
                    src.TraineeGroup.Branch!.Name,
                    src.TraineeGroup.Coach.Employee.FirstName + " " + src.TraineeGroup.Coach.Employee.LastName,
                    src.EnrollmentDate.ToString("yyyy-MM-dd"),
                    src.SubscriptionDetails.StartDate.ToString("yyyy-MM-dd"),
                    src.SubscriptionDetails.EndDate.ToString("yyyy-MM-dd"),
                    src.ExpiryDate.ToString("yyyy-MM-dd"),
                    src.SubscriptionDetails.SportPrice.Price,
                    src.GetPaymentStatus(),
                    src.GetStatus(),
                    GetSessionsCompleted(src.Attendances),
                    src.SessionAllowed - src.SessionRemaining,
                    src.SessionAllowed,
                    src.SubscriptionDetailsId
                ));
        }

        private static int GetSessionsCompleted(ICollection<Attendance> src)
        {
            return src.Count(a =>
                a.AttendanceStatus == AttendanceStatus.Present ||
                a.AttendanceStatus == AttendanceStatus.Late);
        }
    }
}
