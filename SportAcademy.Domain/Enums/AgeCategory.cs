using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Enums
{
    public enum AgeCategory
    {
        Adult = 'A',
        Youth = 'Y',
        Child = 'C'
    }

    public static class AgeCategoryExtensions
    {
        public static char ToChar(this AgeCategory age)
            => (char)age;
    
        public static AgeCategory ToAgeCategory(this char value)
        {
            if (!Enum.IsDefined(typeof(AgeCategory), (int)value))
                throw new ArgumentException($"Invalid age category: {value}");

            return (AgeCategory)value;
        }
    }
}
