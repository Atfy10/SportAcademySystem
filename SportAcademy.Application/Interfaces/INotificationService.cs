using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task SendNotificationToGroupAsync(string groupName, string message);
        Task BroadcastNotificationAsync(string message);
    }
}
