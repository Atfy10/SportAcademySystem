using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Enums
{
    public enum Position
    {
        Coach = 1,
        Manager,
        HR,
        Accountant,
        IT,
        // Generic catch-all for staff who don't hold one of the specialized positions above.
        Employee
    }
}
