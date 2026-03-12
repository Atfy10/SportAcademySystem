using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.Trainees.CreateTrainee
{
    public record CreateTraineeCommand : IRequest<Result<int>>
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string SSN { get; init; }
        public int FamilyId { get; set; }
        public int NationalityCategoryId { get; set; }
        public string? ParentNumber { get; init; }
        public string? GuardianName { get; init; }
        public DateOnly BirthDate { get; init; }
        public Gender Gender { get; init; }
        public string? AppUserId { get; init; }
        public int BranchId { get; init; }
        public HashSet<SportDto> Sports { get; init; } = [];
    }
}
