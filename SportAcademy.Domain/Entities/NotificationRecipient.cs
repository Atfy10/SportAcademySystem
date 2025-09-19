using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class NotificationRecipient
    {
        public bool IsRead { get; set; }
        public int NotificationId { get; set; }
        public required string UserId { get; set; }

        // Navigation Property
        public virtual AppUser User { get; set; } = null!;
        public virtual Notification Notification { get; set; } = null!;

    }
}
