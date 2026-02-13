using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.OpenAiDtos
{
    public record OpenAiMessageDto(string Role, string Content);
}
