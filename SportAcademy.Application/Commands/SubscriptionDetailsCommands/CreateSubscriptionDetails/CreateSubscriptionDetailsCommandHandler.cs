using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISubscriptionTypeRepository _subscriptionTypeRepository;
        private readonly IMapper _mapper;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            ITraineeRepository traineeRepository,
            IPaymentRepository paymentRepository,
            ISubscriptionTypeRepository subscriptionTypeRepository,
            IMapper mapper)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _traineeRepository = traineeRepository;
            _paymentRepository = paymentRepository;
            _subscriptionTypeRepository = subscriptionTypeRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subscriptionDetails = _mapper.Map<SubscriptionDetails>(request);

            var isTraineeExists = await _traineeRepository.IsExistAsync(request.TraineeId,
                cancellationToken);
            if (!isTraineeExists)
                throw new TraineeNotFoundException(request.TraineeId.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var isPaymentNumberExists = await _paymentRepository.IsExistAsync(request.PaymentNumber,
                cancellationToken);
            if (!isPaymentNumberExists)
                throw new PaymentNotFoundException(request.PaymentNumber.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var isSubscriptionTypeExists = await _subscriptionTypeRepository.IsExistAsync(
                request.SubscriptionTypeId, cancellationToken);
            if (!isSubscriptionTypeExists)
                throw new SubscriptionTypeNotFoundException(request.SubscriptionTypeId.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subscriptionDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(subscriptionDetails.Id, _operationType);
        }
    }
}
