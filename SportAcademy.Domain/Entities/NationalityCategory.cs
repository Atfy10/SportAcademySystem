using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Domain.Entities
{
    public class NationalityCategory
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Trainee> Trainees { get; set; } = [];
        public virtual ICollection<NationalityCategoryTranslation> Translations { get; set; } = [];
    }
}
