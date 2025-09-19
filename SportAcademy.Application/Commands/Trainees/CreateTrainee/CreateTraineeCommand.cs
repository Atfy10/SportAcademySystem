using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.Trainees.CreateTrainee
{
    public record CreateTraineeCommand : IRequest<Result<int>>
    {
        public int Id { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string SSN { get; init; }
        public string? ParentNumber { get; init; }
        public string? GuardianName { get; init; }
        public DateOnly BirthDate { get; init; }
        public Gender Gender { get; init; }
        public string? AppUserId { get; init; }
        public int BranchId { get; init; }
        public HashSet<SportDto> Sports { get; init; } = [];
    }
}
