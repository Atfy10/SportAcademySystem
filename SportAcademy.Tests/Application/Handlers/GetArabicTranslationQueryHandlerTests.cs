using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TranslationQueries.GetArabicTranslation;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Tests.Application.Handlers;

// Edit forms used to open with their Arabic fields empty, because the detail endpoints return
// names already resolved to the request language. This query hands back what's actually stored.
public class GetArabicTranslationQueryHandlerTests
{
    private readonly Mock<IBranchRepository> _branches = new();
    private readonly Mock<ISportRepository> _sports = new();
    private readonly Mock<ITraineeGroupRepository> _groups = new();

    private GetArabicTranslationQueryHandler Handler() => new(_branches.Object, _sports.Object, _groups.Object);

    [Fact]
    public async Task Branch_ReturnsTheStoredArabicNameCityAndCountry()
    {
        var branch = new Branch { Id = 3, Name = "Downtown", City = "Kuwait City", Country = "Kuwait", PhoneNumber = "96550000000", CoX = "0", CoY = "0" };
        branch.Translations.Add(new BranchTranslation { LangCode = "en", Name = "Ignored" });
        branch.Translations.Add(new BranchTranslation { LangCode = "ar", Name = "وسط المدينة", City = "مدينة الكويت", Country = "الكويت" });
        _branches.Setup(r => r.GetByIdWithSportsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(branch);

        var result = await Handler().Handle(new GetArabicTranslationQuery(TranslatableEntity.Branch, 3), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(new ArabicTranslationDto("وسط المدينة", "مدينة الكويت", "الكويت", null));
    }

    [Fact]
    public async Task Sport_WithoutArabicTranslation_ReturnsEmptyFields()
    {
        var sport = new Sport { Id = 5, Name = "Swimming" };
        _sports.Setup(r => r.GetByIdWithTranslationsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(sport);

        var result = await Handler().Handle(new GetArabicTranslationQuery(TranslatableEntity.Sport, 5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(new ArabicTranslationDto(null, null, null, null));
    }
}
