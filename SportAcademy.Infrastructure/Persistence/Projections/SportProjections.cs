using System.Linq.Expressions;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>
/// Hand-written translated projections for Sport, used instead of AutoMapper's ProjectTo for the
/// Name/Description fields.
/// </summary>
/// <remarks>
/// AutoMapper Profiles are configured once at startup and ProjectTo compiles a cached expression
/// tree from them - there is no supported way to splice a per-request value into that cached
/// tree. Building the expression here instead, with <paramref name="lang"/> captured as a method
/// parameter, gives EF Core a fresh closure on every call that it correctly parameterizes into
/// SQL - the same pattern that makes `var x = ...; query.Where(e => e.Foo == x)` work everywhere
/// else in EF Core. The same expression compiles and runs equally well over an in-memory
/// IEnumerable, so one definition serves both the ProjectTo (SQL) and Map (in-memory) call sites.
/// </remarks>
public static class SportProjections
{
    public static Expression<Func<Sport, SportDto>> ToDto(string lang) => s => new SportDto(
        s.Id,
        s.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? s.Name,
        s.Translations.Where(t => t.LangCode == lang).Select(t => t.Description).FirstOrDefault() ?? s.Description,
        s.Category,
        s.IsRequireHealthTest);

    public static Expression<Func<Sport, SportDropDownListDto>> ToDropDownDto(string lang) => s => new SportDropDownListDto(
        s.Id,
        s.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? s.Name);
}
