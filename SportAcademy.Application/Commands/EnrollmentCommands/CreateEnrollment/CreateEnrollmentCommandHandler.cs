using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment
{
    public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, Result<int>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ISubscriptionDetailsRepository _subRepository;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateEnrollmentCommandHandler(
            IEnrollmentRepository enrollmentRepository,
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IPublisher publisher)
        {
            _enrollmentRepository = enrollmentRepository;
            _subRepository = subscriptionDetailsRepository;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subRepository.GetSubscriptionDetailsWithSubTypeAsync(
                request.SubscriptionDetailsId, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.SubscriptionDetailsId
                .ToString());

            var enrollment = request.ToEnrollment(subDetails.AllowedSessionsPerMonth);

            cancellationToken.ThrowIfCancellationRequested();

            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new EnrollmentCreatedEvent(enrollment.Id), cancellationToken);

            return Result<int>.Success(enrollment.Id, _operationType);
        }
    }
}
