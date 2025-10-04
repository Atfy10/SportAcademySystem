using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.Trainees.DeleteTrainee
{
    public class DeleteTraineeCommandHandler : IRequestHandler<DeleteTraineeCommand, Result<bool>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly string _operation = OperationType.Delete.ToString();
        public DeleteTraineeCommandHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }
        public async Task<Result<bool>> Handle(DeleteTraineeCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                throw new ArgumentException("Invalid Id");

            await _traineeRepository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true, _operation);
        }
    }
}
