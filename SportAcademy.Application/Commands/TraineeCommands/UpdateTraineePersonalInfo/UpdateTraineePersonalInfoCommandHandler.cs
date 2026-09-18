using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Commands.Trainees.UpdateTraineePersonalInfo
{
    public class UpdateTraineePersonalInfoCommandHandler : IRequestHandler<UpdateTraineePersonalInfoCommand, Result<bool>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineePersonalInfoCommandHandler(
            ITraineeRepository traineeRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            IPhoneNumberNormalizer phoneNormalizer)
        {
            _traineeRepository = traineeRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _phoneNormalizer = phoneNormalizer;
        }

        public async Task<Result<bool>> Handle(UpdateTraineePersonalInfoCommand request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetFullTrainee(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            if (request.ImageUrl != null && request.ImageUrl != trainee.ImageUrl)
                _fileStorage.DeleteImage(trainee.ImageUrl);

            TraineeMapper.ApplyPersonalInfoUpdate(trainee, request);

            // ParentNumber (guardian phone) normalized to E.164 after the mapper applies it -
            // see the matching comment in CreateEmployeeCommandHandler.
            if (!string.IsNullOrWhiteSpace(trainee.ParentNumber))
                trainee.ParentNumber = await _phoneNormalizer.NormalizeAsync(trainee.ParentNumber, cancellationToken);

            // Parity with the pre-split handler: this doesn't depend on anything this command
            // actually changes (PhoneNumber isn't editable here), it just re-checks the
            // trainee's already-persisted number against everyone else's on every personal save.
            var isPhoneNumberExist = await _traineeRepository
                .IsPhoneNumberExistAsync(trainee.PhoneNumber, trainee.Id, cancellationToken);
            if (isPhoneNumberExist)
                throw new PhoneNumberNotUniqueException();

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

            return Result<bool>.Success(true, _operationType);
        }
    }
}
