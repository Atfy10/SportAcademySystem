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
    public record CreateTraineeCommand(
        int Id,
        string FirstName,
        string LastName,
        string SSN,
        string? ParentNumber,
        string? GuardianName,
        DateOnly BirthDate,
        Gender Gender,
        string? AppUserId,
        int BranchId,
        HashSet<SportDto> Sports) : IRequest<Result<int>>;
    //{
    //    public int Id { get; set; }
    //    public required string FirstName { get; set; }
    //    public required string LastName { get; set; }
    //    public required string SSN { get; set; }
    //    public string? ParentNumber { get; set; }
    //    public string? GuardianName { get; set; }
    //    public DateOnly BirthDate { get; set; }
    //    public Gender Gender { get; set; }
    //    public required string AppUserId { get; set; }
    //    public HashSet<SportName> Sports { get; set; } = [];
    //}
}
