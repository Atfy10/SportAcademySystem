using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SessionOccurrenceDtos;

public record SessionOccurrenceCardDto(
       string TraineeGroupName,
       string SportName,
       SessionStatus Status,
       DateTime Date,
       TimeSpan FromTime,
       TimeSpan ToTime,
       string CoachName,
       string BranchName
    );


