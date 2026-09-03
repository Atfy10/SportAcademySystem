using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Enums
{
    public enum SessionStatus
    {
        Scheduled = 1,
        Completed,
        Canceled,
        /// <summary>Cancelled temporarily (e.g. its group was paused) - unlike Canceled, this is
        /// expected to be reverted back to Scheduled (see TraineeGroup pause/resume).</summary>
        CancelledTemporary
    }
}
