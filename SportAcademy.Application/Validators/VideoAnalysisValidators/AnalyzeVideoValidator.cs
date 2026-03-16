using FluentValidation;
using SportAcademy.Application.Commands.VideoAnalysisCommands.AnalyzeVideo;

namespace SportAcademy.Application.Validators.VideoAnalysisValidators;

public class AnalyzeVideoValidator : AbstractValidator<AnalyzeVideoCommand>
{
    public AnalyzeVideoValidator()
    {
        RuleFor(x => x.MovementType)
            .NotEmpty().WithMessage("Movement type is required")
            .MaximumLength(50).WithMessage("Movement type must not exceed 50 characters");

        RuleFor(x => x.Landmarks)
            .NotNull().WithMessage("Landmarks data is required");

        RuleFor(x => x.Landmarks.Frames)
            .NotEmpty().WithMessage("At least one frame of landmarks is required")
            .When(x => x.Landmarks is not null);

        RuleFor(x => x.Landmarks.AverageAngles)
            .NotNull().WithMessage("Average angles are required")
            .When(x => x.Landmarks is not null);

        RuleFor(x => x.Landmarks.TotalFramesProcessed)
            .GreaterThan(0).WithMessage("Total frames must be greater than 0")
            .When(x => x.Landmarks is not null);
    }
}
