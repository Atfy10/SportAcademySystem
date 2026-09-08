using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.Trainees.UpdateTrainee
{
    public record UpdateTraineePersonalCommand : IRequest<Result<UpdateTraineePersonalCommand>>, IBranchScopedRequest, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
        public int Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? GuardianName { get; init; }
        public string? ParentNumber { get; init; }
        public int BranchId { get; init; }
        public List<int> SportIds { get; init; } = [];
        public List<string>? MedicalConditions { get; init; } = [];
        public AppUserDto? AppUser { get; init; }
        public string? ImageUrl { get; init; }
    }
}