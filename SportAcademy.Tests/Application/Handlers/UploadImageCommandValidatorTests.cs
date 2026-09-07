using FluentAssertions;
using SportAcademy.Application.Commands.FileCommands.UploadImage;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class UploadImageCommandValidatorTests
{
    private readonly UploadImageCommandValidator _validator = new();

    private static UploadImageCommand Valid(long length = 1024, string contentType = "image/png") =>
        new(Stream.Null, "photo.png", contentType, length, ImageUploadCategory.Avatar);

    [Fact]
    public void Validate_ValidImage_Passes()
    {
        var result = _validator.Validate(Valid());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("image/svg+xml")]
    public void Validate_UnsupportedContentType_Fails(string contentType)
    {
        var result = _validator.Validate(Valid(contentType: contentType));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContentType");
    }

    [Fact]
    public void Validate_EmptyFile_Fails()
    {
        var result = _validator.Validate(Valid(length: 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length");
    }

    [Fact]
    public void Validate_FileOverFiveMegabytes_Fails()
    {
        var result = _validator.Validate(Valid(length: 5 * 1024 * 1024 + 1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length");
    }

    [Fact]
    public void Validate_FileExactlyFiveMegabytes_Passes()
    {
        var result = _validator.Validate(Valid(length: 5 * 1024 * 1024));

        result.IsValid.Should().BeTrue();
    }
}
