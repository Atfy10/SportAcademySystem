using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.TraineeExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Commands.Trainees.CreateTrainee
{
    public class CreateTraineeCommandHandler : IRequestHandler<CreateTraineeCommand, Result<CreateTraineeResponse>>
    {
        private readonly ITraineeCodeGenerator _traineeCodeGenerator;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ISportRepository _sportRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateTraineeCommandHandler(
            ITraineeCodeGenerator traineeCodeGenerator,
            ITraineeRepository traineeRepository,
            IFamilyRepository familyRepository,
            ISportRepository sportRepository,
            IUnitOfWork unitOfWork,
            IPhoneNumberNormalizer phoneNormalizer,
            IPublisher publisher)
        {
            _traineeCodeGenerator = traineeCodeGenerator;
            _traineeRepository = traineeRepository;
            _familyRepository = familyRepository;
            _sportRepository = sportRepository;
            _unitOfWork = unitOfWork;
            _phoneNormalizer = phoneNormalizer;
            _publisher = publisher;
        }

        public async Task<Result<CreateTraineeResponse>> Handle(CreateTraineeCommand request, CancellationToken cancellationToken)
        {
            var trainee = TraineeMapper.ToEntity(request);

            trainee.JoinDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Normalized to E.164 before the uniqueness/save below - see the matching comment
            // in CreateEmployeeCommandHandler.
            trainee.PhoneNumber = await _phoneNormalizer.NormalizeAsync(trainee.PhoneNumber, cancellationToken)
                ?? trainee.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(trainee.ParentNumber))
                trainee.ParentNumber = await _phoneNormalizer.NormalizeAsync(trainee.ParentNumber, cancellationToken);

            // SSN is optional at creation (e.g. trainee doesn't have one issued yet) - only
            // check uniqueness when one was actually entered. Format/checksum is already
            // enforced by CreateTraineeValidator (ApplyNationalIdRuleFor, country-aware via
            // IRegionalValidationService) in the ValidationBehavior pipeline before this handler
            // ever runs - a second, Kuwait-only PersonValidationHelper.IsValidSSN gate here used
            // to reject every non-Kuwait tenant's already-valid SSN.
            if (!string.IsNullOrWhiteSpace(trainee.SSN))
            {
                var isSSNExist = await _traineeRepository
                    .IsSSNExistAsync(trainee.SSN, cancellationToken);
                if (isSSNExist)
                    throw new SSNNotUniqueException();
            }

            var isPhoneNumberExist = await _traineeRepository
                .IsPhoneNumberExistAsync(trainee.PhoneNumber, cancellationToken: cancellationToken);
            if (isPhoneNumberExist)
                throw new PhoneNumberNotUniqueException();

            var isEmailExist = await _traineeRepository
                .IsEmailExistAsync(trainee.Email.Value, cancellationToken);
            if (isEmailExist)
                throw new EmailExistException();

            var ageCategory = trainee.AgeCategory;
            bool isAdult = ageCategory == AgeCategory.Adult;
            bool isGuardianInfoMissing = (string.IsNullOrWhiteSpace(trainee.ParentNumber)
                || string.IsNullOrWhiteSpace(trainee.GuardianName));
            if (!isAdult && isGuardianInfoMissing)
                throw new GuardianInfoMissingException();

            int familyId;
            if (request.FamilyId > 0)
            {
                var existingFamily = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
                    ?? throw new IdNotFoundException("Family", request.FamilyId.ToString());
                familyId = existingFamily.Id;
            }
            else
            {
                var newFamily = new Family
                {
                    FamilyCode = _familyRepository.SelectNextId(),
                    LastMemberNumber = 0
                };
                await _familyRepository.AddAsyncWithoutSave(newFamily, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                familyId = newFamily.Id;
            }

            var sportIds = request.SportIds.ToList();
            if (sportIds.Count != 0)
            {
                var allSportsExist = await _sportRepository.AreIdsExistAsync(sportIds, cancellationToken);
                var allSports = await _sportRepository.GetAllAsync(cancellationToken);
                var allTraineeSports = new List<SportTrainee>();
                if (!allSportsExist)
                {
                    var validIds = allSports.Select(s => s.Id).ToHashSet();
                    var invalidIds = sportIds.Where(id => !validIds.Contains(id)).ToList();
                    throw new IdNotFoundException("Sport", invalidIds.FirstOrDefault().ToString());
                }

                foreach (var sportId in sportIds)
                {
                    allTraineeSports.Add(
                        new SportTrainee
                        {
                            SportId = sportId,
                            SkillLevel = SkillLevel.NotSpecified
                        }
                    );
                }

                trainee.Sports = allTraineeSports;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var code = await _traineeCodeGenerator.GenerateAsync(
                familyId,
                trainee.BranchId,
                trainee.NationalityCategoryId,
                ageCategory,
                cancellationToken);

            trainee.Id = await CreateTraineeId(trainee);
            trainee.TraineeCode = TraineeCode.FromString(code);
            trainee.FamilyId = familyId;

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.AddAsyncWithoutSave(trainee, cancellationToken);

            if (request.MedicalConditions?.Count > 0)
            {
                foreach (var condition in request.MedicalConditions.Where(c => !string.IsNullOrWhiteSpace(c)))
                {
                    trainee.MedicalConditions.Add(new TraineeMedicalCondition
                    {
                        TraineeId = trainee.Id,
                        Condition = condition.Trim()
                    });
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            // No AppUser is created here - a trainee is a record the academy manages, not
            // someone who signs in to this console, and there is no trainee-facing portal
            // anywhere in the system for such an account to ever be used with. An earlier
            // version of this handler did create one (with a generated username/password
            // returned to the caller and never shown anywhere in the UI); it was inserted
            // straight through the repository rather than UserManager.CreateAsync, so it never
            // got a SecurityStamp - any later UserManager.UpdateAsync on that row (activate/
            // deactivate, edit) threw "User security stamp cannot be null." That dead, silently
            // broken account is the whole reason this comment exists: don't reintroduce it.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new TraineeCreatedEvent(trainee.Id), cancellationToken);

            // Sport assignment here bypasses SportTrainee's own CreateSportTraineeCommandHandler
            // (this handler builds SportTrainee rows directly, above), which is the only other
            // place that normally publishes this event - without it, a trainee's sports would
            // never get an initial skill-history row, and the skill-progress report/edit modal
            // would show them as having no sports at all despite Sports clearly listing some.
            foreach (var sportId in sportIds)
            {
                await _publisher.Publish(
                    new SportTraineeSkillLevelChangedEvent(trainee.Id, sportId, SkillLevel.NotSpecified, SkillLevel.NotSpecified),
                    cancellationToken);
            }

            return Result<CreateTraineeResponse>.Success(
                new CreateTraineeResponse
                {
                    TraineeId = trainee.Id,
                    Code = trainee.TraineeCode.Value
                },
                _operationType,
                "Trainee created successfully"
            );
        }

        private async Task<int> CreateTraineeId(Trainee trainee)
        {
            var year = (trainee.BirthDate.Year % 100);
            var month = (trainee.BirthDate.Month);
            var dobCode = $"{year:D2}{month:D2}";

            var firstLetter = char.ToUpper(trainee.FirstName[0]);
            var ascii = ((int)firstLetter).ToString("D2");

            var prefix = $"{trainee.BranchId}{dobCode}{ascii}";

            var ids = await _traineeRepository.GetIdsAsync();
            var count = ids
                .Where(id => id.ToString().StartsWith(prefix))
                .ToList().Count;

            var counter = (count + 1).ToString("D1");

            var codeString = $"{prefix}{counter}";
            return int.Parse(codeString);
        }
    }
}
