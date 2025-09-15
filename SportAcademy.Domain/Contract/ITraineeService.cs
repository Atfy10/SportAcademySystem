using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Contract
{
    public interface ITraineeService
    {
        int CreateTraineeCode(Trainee trainee, int branchId);
        bool IsSSNValid(string ssn);
        int CalculateAge(DateOnly birthDate);
        bool IsAdult(DateOnly birthDate);
    }
}
