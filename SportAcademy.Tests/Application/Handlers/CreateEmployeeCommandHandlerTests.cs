using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class CreateEmployeeCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _handler = new CreateEmployeeCommandHandler(
            _personServiceMock.Object,
            _mapperMock.Object,
            _employeeRepoMock.Object,
            _userRepoMock.Object);
    }

    private static CreateEmployeeCommand CreateValidCommand() => new(
        FirstName: "Mohammad",
        LastName: "Al-Sabah",
        SSN: "294051512345",
        Salary: 5000m,
        Gender: Gender.Male,
        BirthDate: new DateOnly(1990, 4, 5),
        Email: "mohammad.sabah@academy.com",
        Nationality: "Kuwaiti",
        Street: "Main Street 123",
        City: "Kuwait City",
        PhoneNumber: "51234567",
        SecondNumber: "65234567",
        Position: Position.Manager,
        BranchId: 1
    );

    private static Employee CreateMappedEmployee() => new()
    {
        Id = 1,
        FirstName = "Mohammad",
        LastName = "Al-Sabah",
        SSN = "294051512345",
        Salary = 5000m,
        Gender = Gender.Male,
        BirthDate = new DateOnly(1990, 4, 5),
        PhoneNumber = "51234567",
        SecondPhoneNumber = "65234567",
        Position = Position.Manager,
        BranchId = 1,
        HireDate = DateTime.Now,
        IsWork = true
    };

    private static AppUser CreateMappedAppUser(string userId = "test-user-123") => new()
    {
        Id = userId,
        UserName = "mohammad.al-sabah",
        Email = "mohammad.sabah@academy.com",
        IsBanned = false,
        CreatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    [Fact]
    public async Task Handle_ValidEmployee_ReturnsSuccessResult()
    {
        // Arrange
        var command = CreateValidCommand();
        var employee = CreateMappedEmployee();
        var appUser = CreateMappedAppUser();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(employee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _personServiceMock.Setup(s => s.GenerateUserName(employee.FirstName, employee.LastName)).Returns("mohammad.al-sabah");
        _personServiceMock.Setup(s => s.GeneratePassword()).Returns("TempPass123!");
        _userRepoMock.Setup(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("mohammad.al-sabah", It.IsAny<CancellationToken>())).ReturnsAsync(appUser);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
        _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidSSN_ThrowsSSNSyntaxErrorException()
    {
        // Arrange
        var command = CreateValidCommand();
        var employee = CreateMappedEmployee();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(false);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<SSNSyntaxErrorException>();
    }

    [Fact]
    public async Task Handle_DuplicateSSN_ThrowsSSNNotUniqueException()
    {
        // Arrange
        var command = CreateValidCommand();
        var employee = CreateMappedEmployee();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<SSNNotUniqueException>();
    }

    [Fact]
    public async Task Handle_DuplicatePhoneNumber_ThrowsPhoneNumberNotUniqueException()
    {
        // Arrange
        var command = CreateValidCommand();
        var employee = CreateMappedEmployee();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(employee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<PhoneNumberNotUniqueException>();
    }

    [Fact]
    public async Task Handle_UserRegistrationFails_ThrowsUserRegistrationException()
    {
        // Arrange
        var command = CreateValidCommand();
        var employee = CreateMappedEmployee();
        var errors = new List<Microsoft.AspNetCore.Identity.IdentityError>
        {
            new() { Description = "Password does not meet requirements" }
        };
        var failedResult = Microsoft.AspNetCore.Identity.IdentityResult.Failed(errors.ToArray());

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(employee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _personServiceMock.Setup(s => s.GenerateUserName(employee.FirstName, employee.LastName)).Returns("mohammad.al-sabah");
        _personServiceMock.Setup(s => s.GeneratePassword()).Returns("TempPass123!");
        _userRepoMock.Setup(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(failedResult);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UserRegistrationException>();
    }

    [Fact]
    public async Task Handle_MapperThrows_ThrowsAutoMapperMappingException()
    {
        // Arrange
        var command = CreateValidCommand();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns((Employee?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<AutoMapperMappingException>();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand();
        var employee = CreateMappedEmployee();
        var appUser = CreateMappedAppUser();
        var cancellationTokenSource = new CancellationTokenSource();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(employee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _personServiceMock.Setup(s => s.GenerateUserName(employee.FirstName, employee.LastName)).Returns("mohammad.al-sabah");
        _personServiceMock.Setup(s => s.GeneratePassword()).Returns("TempPass123!");
        _userRepoMock.Setup(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("mohammad.al-sabah", It.IsAny<CancellationToken>())).ReturnsAsync(appUser);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback(() => cancellationTokenSource.Token.ThrowIfCancellationRequested())
            .Returns(Task.CompletedTask);

        cancellationTokenSource.Cancel();

        // Act & Assert
        var act = () => _handler.Handle(command, cancellationTokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(Position.Manager)]
    [InlineData(Position.Coach)]
    [InlineData(Position.HR)]
    public async Task Handle_DifferentPositions_ReturnsSuccess(Position position)
    {
        // Arrange
        var command = CreateValidCommand() with { Position = position };
        var employee = CreateMappedEmployee();
        employee.Position = position;
        var appUser = CreateMappedAppUser();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(employee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _personServiceMock.Setup(s => s.GenerateUserName(employee.FirstName, employee.LastName)).Returns("mohammad.al-sabah");
        _personServiceMock.Setup(s => s.GeneratePassword()).Returns("TempPass123!");
        _userRepoMock.Setup(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("mohammad.al-sabah", It.IsAny<CancellationToken>())).ReturnsAsync(appUser);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_VariousSalaries_ReturnsSuccess()
    {
        // Arrange
        var command = CreateValidCommand() with { Salary = 10000m };
        var employee = CreateMappedEmployee();
        employee.Salary = 10000m;
        var appUser = CreateMappedAppUser();

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _personServiceMock.Setup(s => s.IsSSNValid(employee.SSN, employee.BirthDate)).Returns(true);
        _employeeRepoMock.Setup(r => r.IsSSNExistAsync(employee.SSN, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.IsPhoneNumberExistAsync(employee.PhoneNumber, 0, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _personServiceMock.Setup(s => s.GenerateUserName(employee.FirstName, employee.LastName)).Returns("mohammad.al-sabah");
        _personServiceMock.Setup(s => s.GeneratePassword()).Returns("TempPass123!");
        _userRepoMock.Setup(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("mohammad.al-sabah", It.IsAny<CancellationToken>())).ReturnsAsync(appUser);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        employee.Salary.Should().Be(10000m);
    }
}
