using System.Text.RegularExpressions;
using FluentAssertions;
using SportAcademy.Application.Common.Security;

namespace SportAcademy.Tests.Application.Common;

// AdminCreateUserCommandHandler used to generate its initial password from
// Convert.ToBase64String(RandomNumberGenerator.GetBytes(8)) - a base64 alphabet has no guaranteed
// digit or symbol, so it failed Identity's RequireDigit policy check roughly 1 in 7 times,
// making the "Add User" modal fail intermittently with nothing to point at in the code. This
// runs many iterations (not just one) precisely because the bug it guards against was itself
// probabilistic - a single passing sample would not have caught it.
public class SecurePasswordGeneratorTests
{
    [Fact]
    public void Generate_AlwaysSatisfiesIdentitysDefaultPasswordPolicy()
    {
        for (var i = 0; i < 500; i++)
        {
            var password = SecurePasswordGenerator.Generate();

            password.Length.Should().BeGreaterThanOrEqualTo(8);
            password.Should().MatchRegex("[A-Z]");
            password.Should().MatchRegex("[a-z]");
            password.Should().MatchRegex("[0-9]");
            Regex.IsMatch(password, "[a-zA-Z0-9]").Should().BeTrue();
            password.Any(c => !char.IsLetterOrDigit(c)).Should().BeTrue("Identity requires a non-alphanumeric character");
        }
    }

    [Fact]
    public void Generate_ProducesDistinctPasswordsAcrossCalls()
    {
        var passwords = Enumerable.Range(0, 50).Select(_ => SecurePasswordGenerator.Generate()).ToList();

        passwords.Distinct().Should().HaveCount(passwords.Count);
    }
}
