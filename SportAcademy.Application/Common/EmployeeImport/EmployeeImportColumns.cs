namespace SportAcademy.Application.Common.EmployeeImport
{
    // One importable employee column. Key is the canonical header (what the template writes); a
    // file may use any alias instead - the console export's labels in English or Arabic - so an
    // exported file still imports. "Required" columns must exist in the header row.
    public sealed record EmployeeImportColumn(string Key, bool Required, params string[] Aliases);

    // The same fields the Add Employee form collects (CreateEmployeeCommand), so an imported
    // employee is exactly what the form would have created. No login account is created - accounts
    // are opened per person from the console, where the generated password can be handed over.
    public static class EmployeeImportColumns
    {
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string BirthDate = "BirthDate";
        public const string Gender = "Gender";
        public const string Nationality = "Nationality";
        public const string PhoneNumber = "PhoneNumber";
        public const string SecondPhoneNumber = "SecondPhoneNumber";
        public const string Email = "Email";
        public const string Ssn = "SSN";
        public const string Branch = "Branch";
        public const string Position = "Position";
        public const string Salary = "Salary";
        public const string Street = "Street";
        public const string City = "City";

        public static readonly IReadOnlyList<EmployeeImportColumn> All =
        [
            new(FirstName, true, "first name", "given name"),
            new(LastName, true, "last name", "family name", "surname"),
            new(BirthDate, true, "birth date", "date of birth", "dob", "birthday"),
            new(Gender, true, "sex"),
            new(Nationality, true),
            new(PhoneNumber, true, "phone", "phone number", "mobile", "mobile number"),
            new(Email, true, "e-mail", "email address"),
            new(Ssn, true, "national id", "civil id", "ssn number"),
            new(Branch, true, "branchid", "branch id", "branch name"),
            new(Position, true, "job title", "role", "job"),
            new(Salary, true, "monthly salary", "pay", "wage"),
            new(Street, true, "address", "street address"),
            new(City, true),
            new(SecondPhoneNumber, false, "second phone", "secondphone", "second number", "secondary phone", "other phone"),
        ];
    }
}
