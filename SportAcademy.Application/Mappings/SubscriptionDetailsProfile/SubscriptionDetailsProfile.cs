using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.SubscriptionDetailsProfile
{
    public class SubscriptionDetailsProfile : AutoMapper.Profile
    {
        public SubscriptionDetailsProfile()
        {
            CreateMap<SubscriptionDetails, SubscriptionDetailsDto>()
                .ForPath(
                    dest => dest.Trainee.Id,
                    opt => opt.MapFrom(src => src.Trainee.Id)
                )
                .ForPath(
                    dest => dest.Trainee.FullName,
                    opt => opt.MapFrom(src => $"{src.Trainee.FirstName} {src.Trainee.LastName}")
                )
                .ForPath(
                    dest => dest.Trainee.PhoneNumber,
                    opt => opt.MapFrom(src => src.Trainee.PhoneNumber)
                )
                .ForPath(
                    dest => dest.Payment.BranchName,
                    opt => opt.MapFrom(src => src.Payment.Branch.Name)
                )
                .ForPath(
                    dest => dest.Payment.PaymentNumber,
                    opt => opt.MapFrom(src => src.Payment.PaymentNumber)
                )
                .ForPath(
                    dest => dest.Payment.PaymentMethod,
                    opt => opt.MapFrom(src => src.Payment.Method)
                )
                .ForPath(
                    dest => dest.Payment.PaidDate,
                    opt => opt.MapFrom(src => src.Payment.PaidDate)
                )
                .ForMember(
                    dest => dest.SportName,
                    opt => opt.MapFrom(src => src.SportPrice.SportSubscriptionType.Sport.Name)
                )
                .ForMember(
                    dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.SportPrice.Branch.Name)
                )
                .ForMember(
                    dest => dest.SubscriptionTypeName,
                    opt => opt.MapFrom(src => src.SportPrice.SportSubscriptionType.SubscriptionType.Name)
                )
                .ForMember(
                    dest => dest.Price,
                    opt => opt.MapFrom(src => src.SportPrice.Price)
                )
                .ReverseMap()
                .ForAllMembers(
                    opt => opt.Condition((src, dest, srcMember) => srcMember != null)
                );

            CreateMap<CreateSubscriptionDetailsCommand, SubscriptionDetails>();

            CreateMap<UpdateSubscriptionDetailsCommand, SubscriptionDetails>()
                .ForAllMembers(
                    opt => opt.Condition((src, dest, srcMember) => srcMember != null)
                );
        }
    }
}
