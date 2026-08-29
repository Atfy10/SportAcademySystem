using System.Linq.Expressions;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>Hand-written translated projections for Branch - see SportProjections for why.</summary>
public static class BranchProjections
{
    public static Expression<Func<Branch, BranchCardDto>> ToCardDto(string lang) => b => new BranchCardDto
    {
        Id = b.Id,
        Name = b.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? b.Name,
        City = b.Translations.Where(t => t.LangCode == lang).Select(t => t.City).FirstOrDefault() ?? b.City,
        Country = b.Translations.Where(t => t.LangCode == lang).Select(t => t.Country).FirstOrDefault() ?? b.Country,
        PhoneNumber = b.PhoneNumber,
        Email = b.Email,
        CoX = b.CoX,
        CoY = b.CoY,
    };

    public static Expression<Func<Branch, BranchDropDownListDto>> ToDropDownDto(string lang) => b => new BranchDropDownListDto(
        b.Id,
        b.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? b.Name);
}
