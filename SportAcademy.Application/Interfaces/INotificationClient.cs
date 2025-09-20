using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationClient
    {
        Task RecieveNotification(string message);
    }
}
