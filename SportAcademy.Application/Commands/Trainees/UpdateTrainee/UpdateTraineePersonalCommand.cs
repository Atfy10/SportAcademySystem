using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.Trainees.UpdateTrainee
{
    public record UpdateEmployeePersonalCommand : IRequest<Result<UpdateEmployeePersonalCommand>>
    {
        public int Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? GuardianName { get; init; }
        public string? ParentNumber { get; init; }
        public AppUserDto? AppUser { get; init; }
    }
}