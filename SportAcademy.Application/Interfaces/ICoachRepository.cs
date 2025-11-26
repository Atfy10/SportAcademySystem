using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ICoachRepository : IBaseRepository<Coach, int>
    {
    }
}
