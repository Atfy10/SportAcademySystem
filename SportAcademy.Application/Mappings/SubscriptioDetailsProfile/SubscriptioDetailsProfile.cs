using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.SubscriptioDetailsProfile
{
    public class SubscriptioDetailsProfile : AutoMapper.Profile
    {
        public SubscriptioDetailsProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<SubscriptionDetails, CreateSubscriptionDetailsCommand>()
                .ReverseMap();
        }
    }
}
