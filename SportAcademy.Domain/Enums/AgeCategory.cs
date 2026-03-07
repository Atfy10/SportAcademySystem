using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Domain.Enums
{
    public enum AgeCategory
    {
        Adult = 'A',
        Youth = 'Y',
        Kid = 'K'
    }

    public static class AgeCategoryExtensions
    {
        public static char ToChar(this AgeCategory age)
            => (char)age;

        public static AgeCategory ToAgeCategory(this char value)
            => value switch
            {
                'A' => AgeCategory.Adult,
                'Y' => AgeCategory.Youth,
                'C' => AgeCategory.Kid,
                _ => throw new InvalidTraineeCodeException(value.ToString())
            };
    }
}
