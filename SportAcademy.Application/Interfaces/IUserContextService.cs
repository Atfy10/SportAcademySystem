using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IUserContextService
    {
        public string UserId { get; }
        public List<string> Role { get; }
        public bool IsAuthenticated { get; }
    }
}
