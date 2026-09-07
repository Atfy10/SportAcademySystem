using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly UpdateEmployeeCommandHandler _handler;

    public UpdateEmployeeCommandHandlerTests()
    {
        _handler = new UpdateEmployeeCommandHandler(
            _employeeRepoMock.Object, _unitOfWorkMock.Object, _fileStorageMock.Object);
    }

    private static Employee CreateEmployee(int id = 1, string? imageUrl = null) => new()
    {
        Id = id,
        FirstName = "Sara",
        LastName = "Ahmed",
        SSN = "123456789",
        Email = Email.Create("sara@example.com"),
        Address = Address.Create("Street", "City"),
        PhoneNumber = "+96551234567",
        ImageUrl = imageUrl,
        BranchId = 1,
    };

    [Fact]
    public async Task Handle_NewImageUrl_DeletesTheOldOneAndSetsTheNew()
    {
        var employee = CreateEmployee(imageUrl: "/uploads/people/old.png");
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        var result = await _handler.Handle(
            new UpdateEmployeeCommand(1, ImageUrl: "/uploads/people/new.png"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        employee.ImageUrl.Should().Be("/uploads/people/new.png");
        _fileStorageMock.Verify(f => f.DeleteImage("/uploads/people/old.png"), Times.Once);
    }

    [Fact]
    public async Task Handle_NullImageUrl_LeavesExistingImageUnchangedAndDoesNotDelete()
    {
        var employee = CreateEmployee(imageUrl: "/uploads/people/existing.png");
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        var result = await _handler.Handle(new UpdateEmployeeCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        employee.ImageUrl.Should().Be("/uploads/people/existing.png");
        _fileStorageMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameImageUrlResubmitted_DoesNotDeleteIt()
    {
        var employee = CreateEmployee(imageUrl: "/uploads/people/same.png");
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        await _handler.Handle(new UpdateEmployeeCommand(1, ImageUrl: "/uploads/people/same.png"), CancellationToken.None);

        _fileStorageMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
    }
}
