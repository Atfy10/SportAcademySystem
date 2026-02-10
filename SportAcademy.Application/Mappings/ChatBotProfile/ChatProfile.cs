using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SportAcademy.Domain.Entities;


namespace SportAcademy.Application.Mappings.ChatBotProfile
{
    public class ChatProfile : AutoMapper.Profile
    {
        public ChatProfile()
        {
            CreateMap<ChatMessage, ChatMessageDto>();

            CreateMap<ChatConversation, ChatConversationDto>()
                .ForMember(dest => dest.Messages,
                    opt => opt.MapFrom(src => src.Messages));
        }
    }
}

