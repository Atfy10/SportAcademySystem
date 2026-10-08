using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.TranslationQueries.GetArabicTranslation;

public enum TranslatableEntity
{
    Branch,
    Sport,
    TraineeGroup,
}

/// <summary>
/// The stored Arabic translation of one record, as typed - for its edit form.
/// </summary>
/// <remarks>
/// The detail endpoints return names already resolved to the request's language, so an edit form
/// had no way to show the Arabic name it was about to save over: its Arabic fields always opened
/// empty. Fields that don't apply to the entity (a sport has no city) are always null.
/// </remarks>
public record GetArabicTranslationQuery(TranslatableEntity Entity, int Id) : IRequest<Result<ArabicTranslationDto>>;

public record ArabicTranslationDto(string? Name, string? City, string? Country, string? Description);
