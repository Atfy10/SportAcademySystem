using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.ValueObjects
{
    public sealed class Coordinate : ValueObject
    {
        public string CoX { get; private init; } = null!;
        public string CoY { get; private init; } = null!;

        private Coordinate(string x, string y)
        {
            CoX = x;
            CoY = y;
        }

        public static Coordinate Create(string x, string y)
        {
            return new Coordinate(x, y);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CoX;
            yield return CoY;
        }

        public override string ToString() => $"({CoX}, {CoY})";
    }
}
