using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification, int>
    {
        Task AddWithRecipient(Notification notification, string userId);
    }
}
