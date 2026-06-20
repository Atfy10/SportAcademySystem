using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Domain.Entities
{
    public class Employee : Person
    {
        private Employee(PersonData data, decimal salary, Position position, int branchId)
            : base(data)
        {
            Salary = salary;
            Position = position;
            BranchId = branchId;
            IsWork = true;
            HireDate = DateTime.UtcNow;
        }

        private Employee() { }

        public int Id { get; private set; }
        public decimal Salary { get; private set; }
        public DateTime HireDate { get; private set; }
        public Position Position { get; private set; }
        public bool IsWork { get; private set; }
        public int BranchId { get; private set; }
        public string? AppUserId { get; set; }

        public virtual AppUser? AppUser { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach? Coach { get; set; }

        public static Employee Create(
            PersonData data,
            decimal salary,
            Position position,
            int branchId,
            DateTime? hireDate = null)
        {
            if (!IsSsnValid(data.SSN, data.BirthDate))
                throw new Domain.Exceptions.SharedExceptions.SSNSyntaxErrorException();

            var employee = new Employee(data, salary, position, branchId);

            if (hireDate.HasValue)
                employee.HireDate = hireDate.Value;

            return employee;
        }

        public void AssignUser(string appUserId)
        {
            AppUserId = appUserId;
        }

        public void SetPosition(Position position)
        {
            Position = position;
        }

        public void ToggleIsWork()
        {
            IsWork = !IsWork;
        }
    }
}
