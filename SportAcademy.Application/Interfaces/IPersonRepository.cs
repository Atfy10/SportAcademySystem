using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IPersonRepository
    {
        Task<bool> IsPhoneNumberExistAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default);
    }
}
