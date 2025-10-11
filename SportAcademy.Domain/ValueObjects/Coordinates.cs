using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.ValueObjects
{
    public sealed class Coordinate
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

        public override bool Equals(object? obj)
            => obj is Coordinate coordinates &&
               CoX == coordinates.CoX &&
               CoY == coordinates.CoY;

        public override int GetHashCode() => HashCode.Combine(CoX, CoY);

        public override string ToString() => $"({CoX}, {CoY})";

    }
}
