using FluentValidation;

namespace SportAcademy.Application.Commands.FileCommands.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    // image/jpg is not a real IANA type, but some browsers/clients still send it for a .jpg
    // file - accepted defensively rather than rejecting a perfectly normal upload over a
    // client-side quirk this API doesn't control.
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"];

    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB - plenty for an avatar/logo/photo.

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty();

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage("Only JPEG, PNG, WEBP, or GIF images are allowed.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("The uploaded file is empty.")
            .LessThanOrEqualTo(MaxSizeBytes).WithMessage("Image must be 5 MB or smaller.");
    }
}
