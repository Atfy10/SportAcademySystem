using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails
{
    public class DeleteSubscriptionDetailsCommandHandler : IRequestHandler<DeleteSubscriptionDetailsCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly ITraineeRepository _traineeRepository;

        public DeleteSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService,
            IMapper mapper,
            ITraineeRepository traineeRepository
            )
        {
            _mapper = mapper;
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<bool>> Handle(DeleteSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await  _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionTypeNotFoundException(request.Id.ToString());

            //  subDetails.MarkAsDeleted();

            cancellationToken.ThrowIfCancellationRequested();

            var traineeId = subDetails.TraineeId;

            await _subscriptionDetailsRepository.DeleteAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.RecalculateIsSubscribedAsync(traineeId, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
