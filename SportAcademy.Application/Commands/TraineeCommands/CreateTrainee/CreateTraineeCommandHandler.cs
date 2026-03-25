using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.TraineeExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;
using SportAcademy.Domain.Helpers;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Commands.Trainees.CreateTrainee
{
    public class CreateTraineeCommandHandler : IRequestHandler<CreateTraineeCommand, Result<CreateTraineeResponse>>
    {
        private readonly ITraineeCodeGenerator _traineeCodeGenerator;
        private readonly IMapper _mapper;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateTraineeCommandHandler(
            ITraineeCodeGenerator traineeCodeGenerator,
            IMapper mapper,
            ITraineeRepository traineeRepository,
            IFamilyRepository familyRepository,
            IUserRepository userRepository,
            IPasswordHasher<AppUser> passwordHasher,
            ISportRepository sportRepository)
        {
            _traineeCodeGenerator = traineeCodeGenerator;
            _mapper = mapper;
            _traineeRepository = traineeRepository;
            _familyRepository = familyRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _sportRepository = sportRepository;
        }

        public async Task<Result<CreateTraineeResponse>> Handle(CreateTraineeCommand request, CancellationToken cancellationToken)
        {
            var trainee = _mapper.Map<Trainee>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            trainee.JoinDate = DateOnly.FromDateTime(DateTime.UtcNow);

            if (!PersonValidationHelper.IsValidSSN(trainee.SSN, trainee.BirthDate))
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
                    LastMemberNumber = 0
                };
                await _familyRepository.AddAsyncWithoutSave(newFamily, cancellationToken);
                familyId = _familyRepository.SelectNextId();
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
                familyId,
                trainee.BranchId,
                trainee.NationalityCategoryId,
                ageCategory,
                cancellationToken);

            trainee.TraineeCode = TraineeCode.FromString(code);
            trainee.IsSubscribed = false;
            trainee.FamilyId = familyId;

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.AddAsyncWithoutSave(trainee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            string username = await GenerateUniqueUsername(trainee.FirstName, trainee.LastName, cancellationToken);
            string password = PersonValidationHelper.GeneratePassword();

            var appUser = new AppUser
            {
                UserName = username,
                Email = trainee.Email.Value,
                PhoneNumber = trainee.PhoneNumber,
                IsBanned = false
            };
            appUser.PasswordHash = _passwordHasher.HashPassword(appUser, password);
            appUser.Trainee = trainee;

            await _userRepository.AddAsyncWithoutSave(appUser, cancellationToken);

            await _familyRepository.SaveChangesAsync(cancellationToken);

            return Result<CreateTraineeResponse>.Success(
                new CreateTraineeResponse
                {
                    TraineeId = trainee.Id,
                    Username = username,
                    Password = password
                },
                _operationType,
                "Trainee created successfully"
            );
        }

        private async Task<string> GenerateUniqueUsername(string firstName, string lastName, CancellationToken cancellationToken)
        {
            string baseUsername = $"{firstName.ToLower()}{lastName.ToLower()}";
            string username = baseUsername;
            int suffix = 1;

            while (await _userRepository.IsUsernameExistAsync(username, cancellationToken) && suffix < 100)
            {
                username = $"{baseUsername}{suffix++}";
            }

            if (suffix >= 100)
                throw new InvalidOperationException("Unable to generate unique username");

            return username;
        }
    }
}
