using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.Trainees.UpdateTraineePersonalInfo;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdateTraineePersonalInfoCommandHandlerTests
{
    private readonly Mock<ITraineeRepository> _traineeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly Mock<IPhoneNumberNormalizer> _phoneNormalizerMock = new();
    private readonly UpdateTraineePersonalInfoCommandHandler _handler;

    public UpdateTraineePersonalInfoCommandHandlerTests()
    {
        _phoneNormalizerMock
            .Setup(p => p.NormalizeAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string? phone, CancellationToken _) => phone);

        _handler = new UpdateTraineePersonalInfoCommandHandler(
            _traineeRepoMock.Object,
            _unitOfWorkMock.Object,
            _fileStorageMock.Object,
            _phoneNormalizerMock.Object);
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
    }

    [Fact]
    public async Task Handle_NewImageUrl_DeletesTheOldOneAndSetsTheNew()
    {
        var trainee = CreateTrainee(imageUrl: "/uploads/people/old.png");
        SetupHappyPath(trainee);

        var command = new UpdateTraineePersonalInfoCommand { Id = 1, ImageUrl = "/uploads/people/new.png" };
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

        var command = new UpdateTraineePersonalInfoCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        trainee.ImageUrl.Should().Be("/uploads/people/existing.png");
        _fileStorageMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DoesNotTouchBranchOrSports()
    {
        var trainee = CreateTrainee();
        trainee.BranchId = 7;
        SetupHappyPath(trainee);

        var command = new UpdateTraineePersonalInfoCommand { Id = 1, FirstName = "Sara" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        trainee.BranchId.Should().Be(7);
        _traineeRepoMock.Verify(r => r.UpdateSports(It.IsAny<Trainee>(), It.IsAny<IEnumerable<int>>()), Times.Never);
    }
}
