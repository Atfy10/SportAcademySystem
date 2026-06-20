using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
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

            int familyId;
            if (request.FamilyId > 0)
            {
                var existingFamily = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
                    ?? throw new IdNotFoundException("Family", request.FamilyId.ToString());
                familyId = existingFamily.Id;
            }
            else
            {
                var newFamily = Family.Create();
                await _familyRepository.AddAsyncWithoutSave(newFamily, cancellationToken);
                familyId = newFamily.Id;
            }

            var sportIds = request.SportIds.ToList();
            if (sportIds.Count != 0)
            {
                var allSports = await _sportRepository.GetAllAsync(cancellationToken);
                var validIds = allSports.Select(s => s.Id).ToHashSet();
                var invalidIds = sportIds.Where(id => !validIds.Contains(id)).ToList();
                if (invalidIds.Count != 0)
                    throw new IdNotFoundException("Sport", invalidIds.First().ToString());

                foreach (var sportId in sportIds)
                    trainee.AssignSport(sportId);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var code = await _traineeCodeGenerator.GenerateAsync(
                familyId,
                trainee.BranchId,
                trainee.NationalityCategoryId,
                trainee.AgeCategory,
                cancellationToken);

            var traineeId = await CreateTraineeId(trainee);
            trainee.SetIds(traineeId, familyId);
            trainee.SetTraineeCode(TraineeCode.FromString(code));

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.AddAsyncWithoutSave(trainee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            string username = await GenerateUniqueUsername(trainee.FirstName, trainee.LastName, cancellationToken);
            string password = GeneratePassword();

            var appUser = AppUser.CreateWithTrainee(username, trainee.Email.Value, trainee.PhoneNumber);
            appUser.PasswordHash = _passwordHasher.HashPassword(appUser, password);
            appUser.Trainee = trainee;

            await _userRepository.AddAsyncWithoutSave(appUser, cancellationToken);

            await _familyRepository.SaveChangesAsync(cancellationToken);

            return Result<CreateTraineeResponse>.Success(
                new CreateTraineeResponse
                {
                    TraineeId = trainee.Id,
                    Code = trainee.TraineeCode.Value,
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

        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            return password;
        }
    }
}
