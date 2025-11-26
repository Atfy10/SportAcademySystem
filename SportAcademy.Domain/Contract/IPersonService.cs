using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Contract
{
    public interface IPersonService
    {
        bool IsSSNValid(string ssn, DateOnly birthDate);
        int CalculateAge(DateOnly birthDate);
    }
}
