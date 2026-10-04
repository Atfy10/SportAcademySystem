using System.Security.Claims;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Web.Authorization;

namespace SportAcademy.Tests.Web.Authorization;

public class AccessTokenSessionValidatorTests
{
    private readonly Mock<ISecurityStampCache> _cacheMock = new();
    private readonly Guid _userId = Guid.NewGuid();

    private ClaimsPrincipal Principal(string? stamp)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _userId.ToString()) };
        if (stamp is not null)
            claims.Add(new Claim(JwtTokenService.SecurityStampClaimType, stamp));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    private void UserIs(UserSessionState? state) =>
        _cacheMock.Setup(c => c.GetAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(state);

    [Fact]
    public async Task MatchingStamp_IsValid()
    {
        UserIs(new UserSessionState("stamp-1", IsBanned: false, IsDeleted: false));

        (await AccessTokenSessionValidator.IsSessionValidAsync(Principal("stamp-1"), _cacheMock.Object))
            .Should().BeTrue();
    }

    [Fact]
    public async Task RotatedStamp_IsRejected()
    {
        // The stamp moved on (password/role/permission change) after this token was issued.
        UserIs(new UserSessionState("stamp-2", IsBanned: false, IsDeleted: false));

        (await AccessTokenSessionValidator.IsSessionValidAsync(Principal("stamp-1"), _cacheMock.Object))
            .Should().BeFalse();
    }

    [Fact]
    public async Task TokenWithoutStampClaim_IsRejected_WhenTheUserHasAStamp()
    {
        // A token issued before the claim existed - the client's silent refresh replaces it.
        UserIs(new UserSessionState("stamp-1", IsBanned: false, IsDeleted: false));

        (await AccessTokenSessionValidator.IsSessionValidAsync(Principal(null), _cacheMock.Object))
            .Should().BeFalse();
    }

    [Fact]
    public async Task LegacyUserWithNoStamp_IsValid_WithAnEmptyClaim()
    {
        UserIs(new UserSessionState(null, IsBanned: false, IsDeleted: false));

        (await AccessTokenSessionValidator.IsSessionValidAsync(Principal(""), _cacheMock.Object))
            .Should().BeTrue();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task BannedOrDeletedUser_IsRejected_EvenWithAMatchingStamp(bool isBanned, bool isDeleted)
    {
        UserIs(new UserSessionState("stamp-1", isBanned, isDeleted));

        (await AccessTokenSessionValidator.IsSessionValidAsync(Principal("stamp-1"), _cacheMock.Object))
            .Should().BeFalse();
    }

    [Fact]
    public async Task UnknownUser_IsRejected()
    {
        UserIs(null);

        (await AccessTokenSessionValidator.IsSessionValidAsync(Principal("stamp-1"), _cacheMock.Object))
            .Should().BeFalse();
    }
}
