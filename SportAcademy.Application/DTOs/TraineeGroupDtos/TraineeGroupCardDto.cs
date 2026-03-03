using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupCardDto(
     string Name,
     string CoachName,
     string Address,
     int MaxCapacity,
     int TotalTrainees,
     List<GroupScheduleDto> Schedule
    );
