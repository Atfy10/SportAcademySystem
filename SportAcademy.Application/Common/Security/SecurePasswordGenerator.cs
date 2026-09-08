using System.Security.Cryptography;

namespace SportAcademy.Application.Common.Security;

// A random Convert.ToBase64String(RandomNumberGenerator.GetBytes(n)) password (what
// AdminCreateUserCommandHandler used before this existed) is NOT guaranteed to satisfy Identity's
// configured password policy (Program.cs: RequireDigit/RequireUppercase/RequireLowercase/
// RequireNonAlphanumeric all true) - a base64 alphabet has no guaranteed digit or symbol, so
// roughly 1 in 7 generated passwords failed RequireDigit alone, making "create user" fail
// intermittently with no code-level bug to point at. This generator instead picks one character
// from each required category up front, so every password it returns is guaranteed to pass
// UserManager.CreateAsync's policy check regardless of what that policy is configured to.
public static class SecurePasswordGenerator
{
    // Visually ambiguous characters (I/l/1, O/0) are excluded - this password is handed to an
    // admin to read off a screen and relay manually, not typed by the person who chose it.
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Special = "!@#$%^&*-_=+";
    private const string AllCharacters = Uppercase + Lowercase + Digits + Special;

    public static string Generate(int length = 12)
    {
        if (length < 4)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must fit one character from each required category.");

        var password = new char[length];
        password[0] = PickRandom(Uppercase);
        password[1] = PickRandom(Lowercase);
        password[2] = PickRandom(Digits);
        password[3] = PickRandom(Special);
        for (var i = 4; i < length; i++)
            password[i] = PickRandom(AllCharacters);

        // Fisher-Yates shuffle - otherwise every generated password would predictably start
        // with "<Upper><Lower><Digit><Special>", which is its own weakness even if each
        // individual password is unguessable.
        for (var i = password.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    private static char PickRandom(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
