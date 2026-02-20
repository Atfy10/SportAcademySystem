using FluentAssertions;
using SportAcademy.Application.Common.Pagination;

namespace SportAcademy.Tests.Application.Common;

public class PageRequestTests
{
    [Fact]
    public void Create_WithValidValues_ReturnsCorrectPageAndSize()
    {
        var page = PageRequest.Create(2, 20);

        page.Page.Should().Be(2);
        page.PageSize.Should().Be(20);
    }

    [Fact]
    public void Create_WithNullPage_DefaultsTo1()
    {
        var page = PageRequest.Create(null, 10);

        page.Page.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativePage_DefaultsTo1(int input)
    {
        var page = PageRequest.Create(input, 10);

        page.Page.Should().Be(1);
    }

    [Fact]
    public void Create_WithNullSize_DefaultsTo10()
    {
        var page = PageRequest.Create(1, null);

        page.PageSize.Should().Be(PageRequest.DefaultPageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithZeroOrNegativeSize_DefaultsTo10(int input)
    {
        var page = PageRequest.Create(1, input);

        page.PageSize.Should().Be(PageRequest.DefaultPageSize);
    }

    [Fact]
    public void Create_WithSizeExceedingMax_ClampsToMax()
    {
        var page = PageRequest.Create(1, 500);

        page.PageSize.Should().Be(PageRequest.MaxPageSize);
    }

    [Fact]
    public void Skip_CalculatesCorrectOffset()
    {
        var page = PageRequest.Create(3, 20);

        page.Skip.Should().Be(40); // (3-1) * 20
    }
}
