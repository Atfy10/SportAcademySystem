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
        private readonly  ISubscriptionTypeRepository _subscriptionTypeRepository;
        
        public CreateSubscriptionDetailsCommandHandler( ISubscriptionDetailsRepository subscriptionDetailsRepository,ITraineeRepository traineeRepository ,IPaymentRepository paymentRepository,ISubscriptionTypeRepository subscriptionTypeRepository)
        { 
          
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _traineeRepository = traineeRepository;
            _paymentRepository = paymentRepository;
            _subscriptionTypeRepository= subscriptionTypeRepository;

        }
        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subscriptionDetails = new SubscriptionDetails
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PaymentNumber = request.PaymentNumber,
                TraineeId = request.TraineeId,
                SubscriptionTypeId = request.SubscriptionTypeId
            };
           var trainee = await _traineeRepository.GetByIdAsync(request.TraineeId, cancellationToken);
            if (trainee == null)
            {
              throw new IdNotFoundException(nameof(trainee), request.TraineeId.ToString());
            }
            var isPaymentNumberExists = await _paymentRepository.IsExistByPaymentAsync(request.PaymentNumber, cancellationToken);
            if (!isPaymentNumberExists)
                throw new PaymentNotFoundException();

            var IsSubscriptionTypeExists= await _subscriptionTypeRepository.IsSubscriptionTypeExistAsync(request.SubscriptionTypeId, cancellationToken);
            if (!IsSubscriptionTypeExists)
                throw new SubscriptionTypeNotFoundException(request.SubscriptionTypeId.ToString());
            await _subscriptionDetailsRepository.AddAsync(subscriptionDetails, cancellationToken);
            return Result<int>.Success(subscriptionDetails.Id, _operationType);
        }
    }
}
