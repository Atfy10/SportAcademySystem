using SportAcademy.Domain.Exceptions.UserExceptions;
using System.Text.RegularExpressions;

namespace SportAcademy.Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        private static readonly Regex EmailRegex =
            new("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        private Email(string value = "")
        {
            Value = value;
        }

        public string Value { get; private init; }

        public static Email Create(string value)
        {
            var normalized = Validate(value);
            return new Email(normalized);
        }

        private static string Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new EmailSyntaxIncorrectException();

            var normalized = value.Trim().ToLowerInvariant();

            if (!EmailRegex.IsMatch(normalized))
                throw new EmailSyntaxIncorrectException();

            return normalized;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
