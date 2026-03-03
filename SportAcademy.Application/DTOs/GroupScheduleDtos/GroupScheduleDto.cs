using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.GroupScheduleDtos;

    public record GroupScheduleDto(
           DayOfWeek Day, 
           TimeOnly StartTime
        );

