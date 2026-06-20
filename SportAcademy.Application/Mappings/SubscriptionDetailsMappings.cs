using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class SubscriptionDetailsMappings
    {
        public static SubscriptionDetails ToSubscriptionDetails(this CreateSubscriptionDetailsCommand cmd)
        {
            return SubscriptionDetails.Create(
                cmd.StartDate,
                cmd.EndDate,
                cmd.PaymentNumber,
                cmd.TraineeId,
                cmd.SubscriptionTypeId,
                cmd.SportId,
                cmd.BranchId);
        }

        public static SubscriptionDetailsDto ToDto(this SubscriptionDetails sd)
        {
            return new SubscriptionDetailsDto
            {
                Trainee = new TraineeSubDetailsDto
                {
                    Id = sd.Trainee.Id,
                    FullName = $"{sd.Trainee.FirstName} {sd.Trainee.LastName}",
                    PhoneNumber = sd.Trainee.PhoneNumber
                },
                SportName = sd.SportPrice?.SportSubscriptionType?.Sport?.Name ?? string.Empty,
                BranchName = sd.SportPrice?.Branch?.Name ?? string.Empty,
                SubscriptionTypeName = sd.SportPrice?.SportSubscriptionType?.SubscriptionType?.Name.ToString() ?? string.Empty,
                Price = sd.SportPrice?.Price ?? 0,
                StartDate = sd.StartDate,
                EndDate = sd.EndDate,
                EmployeeName = string.Empty,
                Payment = sd.Payment == null
                    ? new PaymentSubDetailsDto()
                    : new PaymentSubDetailsDto
                    {
                        PaymentNumber = sd.Payment.PaymentNumber,
                        PaidDate = sd.Payment.PaidDate,
                        BranchName = sd.Payment.Branch?.Name ?? string.Empty,
                        PaymentMethod = sd.Payment.Method
                    }
            };
        }
    }
}
