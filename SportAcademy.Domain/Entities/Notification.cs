using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Property
        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = [];
    }
}
