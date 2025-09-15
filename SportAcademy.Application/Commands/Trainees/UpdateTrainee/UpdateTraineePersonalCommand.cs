using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.Trainees.UpdateTrainee
{
    public class UpdateTraineePersonalCommand : IRequest<Result<UpdateTraineePersonalCommand>>
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? GuardianName { get; set; }
        public string? ParentNumber { get; set; }
    }
}