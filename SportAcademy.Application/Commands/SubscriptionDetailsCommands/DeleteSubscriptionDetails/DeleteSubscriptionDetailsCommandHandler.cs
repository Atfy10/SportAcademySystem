using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
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
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public DeleteSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService,
            IMapper mapper,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher
            )
        {
            _mapper = mapper;
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(DeleteSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await  _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionTypeNotFoundException(request.Id.ToString());

            //  subDetails.MarkAsDeleted();

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.DeleteAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContext.UserId is { } userId
                ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
                : "System";
            await _publisher.Publish(new SubscriptionLifecycleEvent(subDetails.Id, "Deleted", actorName), cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
