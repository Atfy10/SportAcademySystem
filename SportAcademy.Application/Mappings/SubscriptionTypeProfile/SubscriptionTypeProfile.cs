using SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.SubscriptionTypeProfile
{
    public class SubscriptionTypeProfile : AutoMapper.Profile
    {
        public SubscriptionTypeProfile()
        {
            CreateMap<SubscriptionType, CreateSubscriptionTypeCommand>()
                .ReverseMap();
        }
    }
}
