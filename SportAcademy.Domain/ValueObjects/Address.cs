using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Domain.ValueObjects
{
    public sealed class Address : ValueObject
    {
        public string Street { get; private init; }
        public string City { get; private init; }

        private Address(string street, string city)
        {
            Street = street.Trim();
            City = city.Trim();
        }

        public static Address Create(string street, string city)
        {
            Validate(street, city);
            return new Address(street, city);
        }

        private static void Validate(string street, string city)
        {
            if (string.IsNullOrWhiteSpace(street)
                || string.IsNullOrWhiteSpace(city))
                throw new InvalidAddressException();
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
        }

        public override string ToString()
            => $"{Street}, {City}";
    }
}
