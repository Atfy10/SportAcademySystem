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
            // Enrollment <-> EnrollmentDto and CreateEnrollmentCommand/UpdateEnrollmentCommand ->
            // Enrollment are no longer AutoMapper mappings - CreateEnrollmentCommandHandler and
            // UpdateEnrollmentCommandHandler use Mappings/Manual/EnrollmentMapper.cs instead.

            // .ForCtorParam() throughout below, not .ConstructUsing() - ConstructUsing opts a
            // mapping out of AutoMapper's LINQ expression-tree translation, which ProjectTo
            // relies on to turn this into a SQL projection (same root cause as the Coach
            // dropdown fix). Plain .ForMember() doesn't work either: these are positional
            // records with no parameterless constructor, so AutoMapper must be told how to fill
            // constructor parameters explicitly via ForCtorParam. GetPaymentStatus()/
            // GetStatus()/session-count logic are inlined as expressions here (instead of
            // calling the entity methods) because a call to an arbitrary C# method can't be
            // translated to SQL either - only the expression body can.
            CreateMap<Enrollment, EnrollmentDataDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("EnrollmentDate", opt => opt.MapFrom(src => src.EnrollmentDate))
                .ForCtorParam("ExpiryDate", opt => opt.MapFrom(src => src.ExpiryDate))
                .ForCtorParam("SessionAllowed", opt => opt.MapFrom(src => src.SessionAllowed))
                .ForCtorParam("SessionRemaining", opt => opt.MapFrom(src => src.SessionRemaining))
                .ForCtorParam("IsActive", opt => opt.MapFrom(src => src.IsActive))
                .ForCtorParam("TraineeName", opt => opt.MapFrom(src => src.Trainee.FirstName + " " + src.Trainee.LastName))
                .ForCtorParam("TraineeGroupCoachName", opt => opt.MapFrom(src => src.TraineeGroup.Coach.Employee.FirstName + " " + src.TraineeGroup.Coach.Employee.LastName))
                .ForCtorParam("SubscriptionDetailsId", opt => opt.MapFrom(src => src.SubscriptionDetailsId));

            CreateMap<Enrollment, EnrollmentCardDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("TraineeName", opt => opt.MapFrom(src => src.Trainee.FirstName + " " + src.Trainee.LastName))
                .ForCtorParam("TraineeEmail", opt => opt.MapFrom(src => src.Trainee.AppUser.Email ?? ""))
                .ForCtorParam("Sport", opt => opt.MapFrom(src => src.TraineeGroup!.Coach!.Sport!.Name))
                .ForCtorParam("Program", opt => opt.MapFrom(src => src.SubscriptionDetails.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString()))
                .ForCtorParam("Branch", opt => opt.MapFrom(src => src.TraineeGroup.Branch!.Name))
                .ForCtorParam("CoachName", opt => opt.MapFrom(src => src.TraineeGroup.Coach.Employee.FirstName + " " + src.TraineeGroup.Coach.Employee.LastName))
                .ForCtorParam("EnrollmentDate", opt => opt.MapFrom(src => src.EnrollmentDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("StartDate", opt => opt.MapFrom(src => src.SubscriptionDetails.StartDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("EndDate", opt => opt.MapFrom(src => src.SubscriptionDetails.EndDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("MonthlyFee", opt => opt.MapFrom(src => src.SubscriptionDetails.SportPrice.Price))
                .ForCtorParam("PaymentStatus", opt => opt.MapFrom(src =>
                    src.ExpiryDate < DateTime.UtcNow ? "Overdue" :
                    (src.SubscriptionDetails != null &&
                        src.SubscriptionDetails.InvoiceLines.Any(l => l.Invoice.Status == InvoiceStatus.Paid)) ? "Paid" :
                    "Pending"))
                .ForCtorParam("Status", opt => opt.MapFrom(src =>
                    src.ExpiryDate < DateTime.UtcNow ? "Expired" :
                    !src.IsActive ? "Suspended" :
                    "Active"))
                .ForCtorParam("SessionsCompleted", opt => opt.MapFrom(src => src.Attendances.Count(a =>
                    a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.Late)))
                .ForCtorParam("TotalSessions", opt => opt.MapFrom(src => src.SessionAllowed))
                .ForCtorParam("SessionRemaining", opt => opt.MapFrom(src => src.SessionRemaining))
                .ForCtorParam("TraineeGroupId", opt => opt.MapFrom(src => src.TraineeGroupId))
                .ForCtorParam("SportId", opt => opt.MapFrom(src => src.TraineeGroup.Coach.SportId));

            CreateMap<Enrollment, EnrollmentDetailDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("TraineeName", opt => opt.MapFrom(src => src.Trainee.FirstName + " " + src.Trainee.LastName))
                .ForCtorParam("TraineeEmail", opt => opt.MapFrom(src => src.Trainee.AppUser.Email ?? ""))
                .ForCtorParam("Sport", opt => opt.MapFrom(src => src.TraineeGroup!.Coach!.Sport!.Name))
                .ForCtorParam("Program", opt => opt.MapFrom(src => src.SubscriptionDetails.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString()))
                .ForCtorParam("Branch", opt => opt.MapFrom(src => src.TraineeGroup.Branch!.Name))
                .ForCtorParam("CoachName", opt => opt.MapFrom(src => src.TraineeGroup.Coach.Employee.FirstName + " " + src.TraineeGroup.Coach.Employee.LastName))
                .ForCtorParam("EnrollmentDate", opt => opt.MapFrom(src => src.EnrollmentDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("StartDate", opt => opt.MapFrom(src => src.SubscriptionDetails.StartDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("EndDate", opt => opt.MapFrom(src => src.SubscriptionDetails.EndDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("ExpiryDate", opt => opt.MapFrom(src => src.ExpiryDate.ToString("yyyy-MM-dd")))
                .ForCtorParam("MonthlyFee", opt => opt.MapFrom(src => src.SubscriptionDetails.SportPrice.Price))
                .ForCtorParam("PaymentStatus", opt => opt.MapFrom(src =>
                    src.ExpiryDate < DateTime.UtcNow ? "Overdue" :
                    (src.SubscriptionDetails != null &&
                        src.SubscriptionDetails.InvoiceLines.Any(l => l.Invoice.Status == InvoiceStatus.Paid)) ? "Paid" :
                    "Pending"))
                .ForCtorParam("Status", opt => opt.MapFrom(src =>
                    src.ExpiryDate < DateTime.UtcNow ? "Expired" :
                    !src.IsActive ? "Suspended" :
                    "Active"))
                .ForCtorParam("SessionsCompleted", opt => opt.MapFrom(src => src.Attendances.Count(a =>
                    a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.Late)))
                .ForCtorParam("TotalSessions", opt => opt.MapFrom(src => src.SessionAllowed - src.SessionRemaining))
                .ForCtorParam("SessionAllowed", opt => opt.MapFrom(src => src.SessionAllowed))
                .ForCtorParam("SubscriptionDetailsId", opt => opt.MapFrom(src => src.SubscriptionDetailsId))
                .ForCtorParam("SessionRemaining", opt => opt.MapFrom(src => src.SessionRemaining))
                .ForCtorParam("TraineeGroupId", opt => opt.MapFrom(src => src.TraineeGroupId))
                .ForCtorParam("SportId", opt => opt.MapFrom(src => src.TraineeGroup.Coach.SportId));
        }
    }
}
