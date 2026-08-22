using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails;
using SportAcademy.Application.DTOs.PaymentDtos;
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
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id)
                )
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
                .ForMember(
                    // The most recently received payment allocated to this subscription's
                    // invoice, or null if none has been recorded yet - a subscription is no
                    // longer guaranteed a Payment the instant it's created (see
                    // Finance.Invoice/PaymentAllocation).
                    dest => dest.Payment,
                    opt => opt.MapFrom(src => src.InvoiceLines
                        .SelectMany(l => l.Invoice.Allocations)
                        .Select(a => a.Payment)
                        .OrderByDescending(p => p.PaidDate)
                        .Select(p => new PaymentSubDetailsDto
                        {
                            PaymentNumber = p.PaymentNumber,
                            PaidDate = p.PaidDate,
                            BranchName = p.Branch.Name,
                            PaymentMethod = p.Method,
                        })
                        .FirstOrDefault())
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
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status)
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

            // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out of
            // AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to turn
            // this into a SQL projection (same root cause as the Coach dropdown fix). Plain
            // .ForMember() doesn't work either: SubscriptionDetailsDropdownDto is a positional
            // record with no parameterless constructor, so AutoMapper must be told how to fill
            // constructor parameters explicitly via ForCtorParam.
            CreateMap<SubscriptionDetails, SubscriptionDetailsDropdownDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("Name", opt => opt.MapFrom(src => src.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString()));
        }
    }
}
