using System.Linq.Expressions;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>
/// Hand-written translated projection for Family's own Name/GuardianName - see SportProjections
/// for why. FamilyDetailDto's Members list (age/subscription computation) is left to the
/// existing AutoMapper ProjectTo path since it has no translatable fields of its own beyond
/// BranchName (out of scope here); GetFamilyByIdQueryHandler overlays the translated Name the
/// same way GetTraineeGroupByIdQueryHandler does.
/// </summary>
public static class FamilyProjections
{
    public static Expression<Func<Family, FamilyDto>> ToDto(string lang) => f => new FamilyDto(
        f.Id,
        f.FamilyCode,
        f.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? f.Name,
        f.Translations.Where(t => t.LangCode == lang).Select(t => t.GuardianName).FirstOrDefault() ?? f.GuardianName,
        f.GuardianPhone,
        f.Members.Count);
}
