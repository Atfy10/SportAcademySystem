using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Mappings.EmployeeProfile;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using Xunit;

namespace SportAcademy.Tests.Application.Mappings;

// Regression coverage: EmployeeMappingProfile once configured Employee -> EmployeeDto with
// .ForMember(...) + .ReverseMap(), which throws at runtime for this destination (an immutable
// record with no parameterless constructor) - see EmployeeMappingProfile.cs for the fix
// (.ForCtorParam(), no ReverseMap()). AutoMapperFullTests' AssertConfigurationIsValid() doesn't
// catch this class of bug reliably (it validates config shape, not every runtime mapping
// strategy), so this exercises the actual Map()/ProjectTo() calls the real handlers make.
public class EmployeeDtoMappingProbeTests
{
    [Fact]
    public void Map_EmployeeToEmployeeDto_DoesNotThrow()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        var config = new MapperConfiguration(cfg => cfg.AddProfile<EmployeeMappingProfile>(), loggerFactory);
        var mapper = config.CreateMapper();

        var employee = new Employee
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            SSN = "12345678901234",
            Salary = 100,
            HireDate = DateTime.UtcNow,
            Address = Address.Create("Street 1", "City 1"),
            PhoneNumber = "12345678",
            Position = Position.Coach,
            BranchId = 1,
            Branch = new Branch { Id = 1, Name = "Test Branch", City = "City", Country = "Country", PhoneNumber = "123" },
        };
        typeof(Person).GetProperty(nameof(Person.Email))!.SetValue(employee, Email.Create("a@b.com"));
        typeof(Person).GetProperty(nameof(Person.Gender))!.SetValue(employee, Gender.Male);
        typeof(Person).GetProperty(nameof(Person.Nationality))!.SetValue(employee, Nationality.Other);
        typeof(Person).GetProperty(nameof(Person.BirthDate))!.SetValue(employee, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)));

        var act = () => mapper.Map<EmployeeDto>(employee);

        act.Should().NotThrow();
        var dto = act();
        dto.BranchName.Should().Be("Test Branch");
        dto.Street.Should().Be("Street 1");
        dto.City.Should().Be("City 1");
        dto.Email.Should().Be("a@b.com");
    }

    // EmployeeCardDto is the OTHER Employee->record mapping in the same profile, reached only
    // via ProjectTo (LINQ expression projection) rather than Map() - a different AutoMapper
    // code path that may or may not share EmployeeDto's "needs a parameterless constructor"
    // failure. Confirming it separately rather than assuming it's fine by analogy.
    [Fact]
    public void ProjectTo_EmployeeToEmployeeCardDto_DoesNotThrow()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        var config = new MapperConfiguration(cfg => cfg.AddProfile<EmployeeMappingProfile>(), loggerFactory);

        var employee = new Employee
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            SSN = "12345678901234",
            Salary = 100,
            HireDate = DateTime.UtcNow,
            Address = Address.Create("Street 1", "City 1"),
            PhoneNumber = "12345678",
            Position = Position.Coach,
            BranchId = 1,
            Branch = new Branch { Id = 1, Name = "Test Branch", City = "City", Country = "Country", PhoneNumber = "123" },
        };
        typeof(Person).GetProperty(nameof(Person.Email))!.SetValue(employee, Email.Create("a@b.com"));
        typeof(Person).GetProperty(nameof(Person.Gender))!.SetValue(employee, Gender.Male);
        typeof(Person).GetProperty(nameof(Person.Nationality))!.SetValue(employee, Nationality.Other);
        typeof(Person).GetProperty(nameof(Person.BirthDate))!.SetValue(employee, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)));

        var employees = new List<Employee> { employee }.AsQueryable();

        var act = () => employees.ProjectTo<EmployeeCardDto>(config).ToList();

        act.Should().NotThrow();
        var dto = act().Single();
        dto.BranchName.Should().Be("Test Branch");
    }
}
