using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Services
{
    public class PersonService : IPersonService
    {
        public int CalculateAge(DateOnly birthDate)
        {
            throw new NotImplementedException();
        }

        public bool IsSSNValid(string ssn, DateOnly birthDate)
        {
            return PersonValidationHelper.IsValidSSN(ssn, birthDate);
        }
    }
}
