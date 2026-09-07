using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdateTraineePersonalCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<ITraineeService> _traineeServiceMock = new();
    private readonly Mock<ITraineeRepository> _traineeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly UpdateTraineePersonalCommandHandler _handler;

    public UpdateTraineePersonalCommandHandlerTests()
    {
        _handler = new UpdateTraineePersonalCommandHandler(
            _branchRepoMock.Object,
            _traineeServiceMock.Object,
            _traineeRepoMock.Object,
            _unitOfWorkMock.Object,
            _publisherMock.Object,
            _fileStorageMock.Object);
    }

    private static Trainee CreateTrainee(int id = 1, string? imageUrl = null) => new()
    {
        Id = id,
        FirstName = "Ali",
        LastName = "Hassan",
        SSN = "123456789",
        Email = Email.Create("ali@example.com"),
        Address = Address.Create("Street", "City"),
        PhoneNumber = "+96551234567",
        ImageUrl = imageUrl,
        BranchId = 1,
        MedicalConditions = [],
    };

    private void SetupHappyPath(Trainee trainee)
    {
        _traineeRepoMock.Setup(r => r.GetFullTrainee(trainee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(trainee);
        _traineeRepoMock
            .Setup(r => r.IsPhoneNumberExistAsync(trainee.PhoneNumber, trainee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsExistAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _traineeRepoMock
            .Setup(r => r.GetSportIdsByTraineeId(trainee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _traineeRepoMock
            .Setup(r => r.UpdateSports(trainee, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task Handle_NewImageUrl_DeletesTheOldOneAndSetsTheNew()
    {
        var trainee = CreateTrainee(imageUrl: "/uploads/people/old.png");
        SetupHappyPath(trainee);

        var command = new UpdateTraineePersonalCommand { Id = 1, BranchId = 1, ImageUrl = "/uploads/people/new.png" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        trainee.ImageUrl.Should().Be("/uploads/people/new.png");
        _fileStorageMock.Verify(f => f.DeleteImage("/uploads/people/old.png"), Times.Once);
    }

    [Fact]
    public async Task Handle_NullImageUrl_LeavesExistingImageUnchangedAndDoesNotDelete()
    {
        var trainee = CreateTrainee(imageUrl: "/uploads/people/existing.png");
        SetupHappyPath(trainee);

        var command = new UpdateTraineePersonalCommand { Id = 1, BranchId = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        trainee.ImageUrl.Should().Be("/uploads/people/existing.png");
        _fileStorageMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
    }
}
