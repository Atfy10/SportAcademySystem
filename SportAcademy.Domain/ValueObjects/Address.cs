using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Domain.ValueObjects
{
    public sealed class Address : IEquatable<Address>
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

        public bool Equals(Address? other)
            => other is not null
                && Street == other.Street
                && City == other.City;

        override public bool Equals(object? obj)
            => Equals(obj as Address);

        override public int GetHashCode()
            => HashCode.Combine(Street, City);

        override public string ToString()
            => $"{Street}, {City}";
    }
}
