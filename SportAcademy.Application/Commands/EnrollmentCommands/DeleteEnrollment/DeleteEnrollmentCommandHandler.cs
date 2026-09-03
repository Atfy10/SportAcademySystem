using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.DeleteEnrollment
{
    public class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand, Result<bool>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteEnrollmentCommandHandler(
            IEnrollmentRepository enrollmentRepository,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _enrollmentRepository = enrollmentRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await ((IBaseRepository<Domain.Entities.Enrollment, int>)_enrollmentRepository)
                .GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EnrollmentNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _enrollmentRepository.DeleteAsync(enrollment, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContext.UserId is { } userId
                ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
                : "System";
            await _publisher.Publish(new EnrollmentLifecycleEvent(enrollment.Id, "Deleted", actorName), cancellationToken);

            return Result<bool>.Success(true, _operationType);
        }
    }
}
