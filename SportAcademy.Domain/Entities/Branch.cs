namespace SportAcademy.Domain.Entities
{
    public class Branch
    {
        private Branch() { }

        private Branch(string name, string city, string country, string phoneNumber, string? email, string coX, string coY)
        {
            Name = name;
            City = city;
            Country = country;
            PhoneNumber = phoneNumber;
            Email = email;
            CoX = coX;
            CoY = coY;
            IsActive = true;
        }

        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public string Country { get; private set; } = null!;
        public string PhoneNumber { get; private set; } = null!;
        public string? Email { get; private set; }
        public string CoX { get; private set; } = null!;
        public string CoY { get; private set; } = null!;
        public bool IsActive { get; private set; }

        public virtual ICollection<Trainee> Trainees { get; private set; } = [];
        public virtual ICollection<TraineeGroup> TraineeGroups { get; private set; } = [];
        public virtual ICollection<Employee> Employees { get; private set; } = [];
        public virtual ICollection<SportBranch> Sports { get; private set; } = [];
        public virtual ICollection<SportPrice> SportPrices { get; private set; } = [];
        public virtual ICollection<Payment> Payments { get; private set; } = [];

        public static Branch Create(string name, string city, string country, string phoneNumber, string? email, string coX, string coY)
        {
            return new Branch(name, city, country, phoneNumber, email, coX, coY);
        }

        public void Update(string name, string city, string country, string phoneNumber, string? email, string coX, string coY, bool isActive)
        {
            Name = name;
            City = city;
            Country = country;
            PhoneNumber = phoneNumber;
            Email = email;
            CoX = coX;
            CoY = coY;
            IsActive = isActive;
        }

        public void ToggleStatus()
        {
            IsActive = !IsActive;
        }
    }
}
