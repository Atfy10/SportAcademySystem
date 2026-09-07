using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.FileCommands.UploadImage;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class UploadImageCommandHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly UploadImageCommandHandler _handler;

    public UploadImageCommandHandlerTests()
    {
        _handler = new UploadImageCommandHandler(_fileStorageMock.Object);
    }

    [Fact]
    public async Task Handle_DelegatesToFileStorageAndReturnsItsUrl()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        _fileStorageMock
            .Setup(s => s.SaveImageAsync(stream, "photo.jpg", ImageUploadCategory.Avatar, It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/avatars/abc123.jpg");

        var result = await _handler.Handle(
            new UploadImageCommand(stream, "photo.jpg", "image/jpeg", 3, ImageUploadCategory.Avatar),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Url.Should().Be("/uploads/avatars/abc123.jpg");
    }
}
