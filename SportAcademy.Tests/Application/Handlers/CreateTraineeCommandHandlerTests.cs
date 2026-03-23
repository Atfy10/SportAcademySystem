using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Application.Handlers;

public class CreateTraineeCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ITraineeCodeGenerator> _traineeCodeGenerator = new();
    private readonly Mock<ITraineeService> _traineeServiceMock = new();
    private readonly Mock<ITraineeRepository> _traineeRepoMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();
    private readonly Mock<ISportRepository> _sportRepoMock = new();
    private readonly CreateTraineeCommandHandler _handler;

    public CreateTraineeCommandHandlerTests()
    {
        _handler = new CreateTraineeCommandHandler(
            _traineeCodeGenerator.Object,
            _traineeServiceMock.Object,
            _mapperMock.Object,
            _traineeRepoMock.Object,
            _familyRepoMock.Object,
            _sportRepoMock.Object);
    }

    private static CreateTraineeCommand CreateValidCommand() => new()
    {
        FirstName = "Ahmed",
        LastName = "Al-Mutairi",
        SSN = "304031512345",
        BirthDate = new DateOnly(2004, 3, 15),
        Gender = Gender.Male,
        BranchId = 1,
        NationalityCategoryId = 1,
        FamilyId = 0,
        PhoneNumber = "51234567",
        Email = "ahmed@example.com",
        Nationality = Nationality.Kuwaiti,
        SportIds = [1]
    };

    private static Trainee CreateMappedTrainee() => new()
    {
        FirstName = "Ahmed",
        LastName = "Al-Mutairi",
        SSN = "304031512345",
        PhoneNumber = "51234567",
        Email = Email.Create("ahmed@example.com"),
        BirthDate = new DateOnly(2004, 3, 15),
        Gender = Gender.Male,
        Nationality = Nationality.Kuwaiti,
        Address = Address.Create("Main Street", "Kuwait City")
    };

    [Fact]
    public async Task Handle_ValidAdultTrainee_ReturnsSuccessResult()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeServiceMock.Setup(s => s.CreateTraineeCode(trainee, command.BranchId)).Returns(104036501);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeServiceMock.Setup(s => s.IsAdult(trainee.BirthDate)).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(104036501);
        _traineeRepoMock.Verify(r => r.AddAsync(trainee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidSSN_ThrowsException()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_DuplicateSSN_ThrowsException()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_DuplicatePhoneNumber_ThrowsException()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_MinorWithoutGuardianInfo_ThrowsException()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeServiceMock.Setup(s => s.CreateTraineeCode(trainee, command.BranchId)).Returns(104036501);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeServiceMock.Setup(s => s.IsAdult(trainee.BirthDate)).Returns(false);
        // trainee.ParentNumber and GuardianName are null by default

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_MinorWithGuardianInfo_ReturnsSuccessResult()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();
        trainee.ParentNumber = "65234567";
        trainee.GuardianName = "Ahmed Al-Mutairi";

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeServiceMock.Setup(s => s.CreateTraineeCode(trainee, command.BranchId)).Returns(104036501);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeServiceMock.Setup(s => s.IsAdult(trainee.BirthDate)).Returns(false);
        _traineeRepoMock.Setup(r => r.AddAsync(trainee, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(104036501);
        trainee.IsSubscribed.Should().BeFalse();
        _traineeRepoMock.Verify(r => r.AddAsync(trainee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MapperThrows_ThrowsAutoMapperMappingException()
    {
        var command = CreateValidCommand();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns((Trainee?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AutoMapperMappingException>();
    }

    [Fact]
    public async Task Handle_SuccessfulCreation_SetsIsSubscribedToFalse()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();
        trainee.IsSubscribed = true; // Set to true initially

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeServiceMock.Setup(s => s.CreateTraineeCode(trainee, command.BranchId)).Returns(104036501);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeServiceMock.Setup(s => s.IsAdult(trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.AddAsync(trainee, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        trainee.IsSubscribed.Should().BeFalse(); // Should be set to false
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        var command = CreateValidCommand();
        var trainee = CreateMappedTrainee();
        var cancellationTokenSource = new CancellationTokenSource();

        _mapperMock.Setup(m => m.Map<Trainee>(command)).Returns(trainee);
        _traineeServiceMock.Setup(s => s.IsSSNValid(trainee.SSN, trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.IsSSNExistAsync(trainee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _traineeServiceMock.Setup(s => s.CreateTraineeCode(trainee, command.BranchId)).Returns(104036501);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sportRepoMock.Setup(r => r.AreIdsExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeServiceMock.Setup(s => s.IsAdult(trainee.BirthDate)).Returns(true);
        _traineeRepoMock.Setup(r => r.AddAsync(trainee, It.IsAny<CancellationToken>()))
            .Callback(() => cancellationTokenSource.Token.ThrowIfCancellationRequested())
            .Returns(Task.CompletedTask);

        cancellationTokenSource.Cancel();

        var act = () => _handler.Handle(command, cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
