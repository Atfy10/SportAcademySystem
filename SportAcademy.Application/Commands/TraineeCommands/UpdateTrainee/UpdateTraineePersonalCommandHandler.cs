using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Commands.Trainees.UpdateTrainee
{
    public class UpdateTraineePersonalCommandHandler : IRequestHandler<UpdateTraineePersonalCommand, Result<UpdateTraineePersonalCommand>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ITraineeService _traineeService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineePersonalCommandHandler(
            IBranchRepository branchRepository,
            ITraineeService traineeService,
            ITraineeRepository traineeRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _branchRepository = branchRepository;
            _traineeService = traineeService;
            _traineeRepository = traineeRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<UpdateTraineePersonalCommand>> Handle(UpdateTraineePersonalCommand request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetFullTrainee(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            TraineeMapper.ApplyPersonalUpdate(trainee, request);

            var isPhoneNumberExist = await _traineeRepository
                .IsPhoneNumberExistAsync(trainee.PhoneNumber, trainee.Id, cancellationToken);
            if (isPhoneNumberExist)
                throw new PhoneNumberNotUniqueException();

            var isBranchExist = await _branchRepository.IsExistAsync(request.BranchId, cancellationToken);
            if (!isBranchExist)
                throw new BranchNotFoundException(request.BranchId.ToString());

            var currentSportIds = await _traineeRepository
                .GetSportIdsByTraineeId(request.Id, cancellationToken);

            var addedSportIds = await _traineeRepository.UpdateSports(trainee, request.SportIds);

            if (request.MedicalConditions != null)
            {
                var existingConditions = trainee.MedicalConditions.ToList();
                var newConditions = request.MedicalConditions
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .Distinct()
                    .ToList();

                var toRemove = existingConditions
                    .Where(ec => !newConditions.Contains(ec.Condition))
                    .ToList();
                foreach (var rm in toRemove)
                    trainee.MedicalConditions.Remove(rm);

                var existingValues = existingConditions.Select(ec => ec.Condition).ToHashSet();
                var toAdd = newConditions
                    .Where(nc => !existingValues.Contains(nc))
                    .ToList();
                foreach (var cond in toAdd)
                    trainee.MedicalConditions.Add(new TraineeMedicalCondition
                    {
                        TraineeId = trainee.Id,
                        Condition = cond
                    });
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.UpdateAsyncWithoutSave(trainee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // See CreateTraineeCommandHandler for why this is needed: UpdateSports bypasses
            // SportTrainee's own CreateSportTraineeCommandHandler, so without this, a sport
            // added to a trainee here would never get an initial skill-history row.
            foreach (var sportId in addedSportIds)
            {
                await _publisher.Publish(
                    new SportTraineeSkillLevelChangedEvent(trainee.Id, sportId, SkillLevel.NotSpecified, SkillLevel.NotSpecified),
                    cancellationToken);
            }

            return Result<UpdateTraineePersonalCommand>.Success(request, _operationType);
        }
    }
}
