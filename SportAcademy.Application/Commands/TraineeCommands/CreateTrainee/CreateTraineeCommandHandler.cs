using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
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
    public class CreateTraineeCommandHandler : IRequestHandler<CreateTraineeCommand, Result<int>>
    {
        private readonly ITraineeCodeGenerator _traineeCodeGenerator;
        private readonly IMapper _mapper;
        private readonly ITraineeService _traineeService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateTraineeCommandHandler(
            ITraineeCodeGenerator traineeCodeGenerator,
            ITraineeService traineeService,
            IMapper mapper,
            ITraineeRepository traineeRepository,
            IFamilyRepository familyRepository,
            ISportRepository sportRepository)
        {
            _traineeCodeGenerator = traineeCodeGenerator;
            _mapper = mapper;
            _traineeService = traineeService;
            _traineeRepository = traineeRepository;
            _familyRepository = familyRepository;
            _sportRepository = sportRepository;
        }

        public async Task<Result<int>> Handle(CreateTraineeCommand request, CancellationToken cancellationToken)
        {
            var trainee = _mapper.Map<Trainee>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            trainee.JoinDate = DateOnly.FromDateTime(DateTime.UtcNow);

            if (!_traineeService.IsSSNValid(trainee.SSN, trainee.BirthDate))
                throw new SSNSyntaxErrorException();

            var isSSNExist = await _traineeRepository
                .IsSSNExistAsync(trainee.SSN, cancellationToken);
            if (isSSNExist)
                throw new SSNNotUniqueException();

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

            if (request.FamilyId > 0)
            {
                var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
                    ?? throw new IdNotFoundException("Family", request.FamilyId.ToString());
            }

            var sportIds = request.SportIds.ToList();
            if (sportIds.Count != 0)
            {
                var allSportsExist = await _sportRepository.AreIdsExistAsync(sportIds, cancellationToken);
                if (!allSportsExist)
                {
                    var allSports = await _sportRepository.GetAllAsync(cancellationToken);
                    var validIds = allSports.Select(s => s.Id).ToHashSet();
                    var invalidIds = sportIds.Where(id => !validIds.Contains(id)).ToList();
                    throw new IdNotFoundException("Sport", invalidIds.FirstOrDefault().ToString());
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            var code = await _traineeCodeGenerator.GenerateAsync(
                trainee.FamilyId,
                trainee.BranchId,
                trainee.NationalityCategoryId,
                ageCategory,
                cancellationToken);

            trainee.TraineeCode = TraineeCode.FromString(code);
            trainee.IsSubscribed = false;

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.AddAsync(trainee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(trainee.Id, _operationType);
        }
    }
}