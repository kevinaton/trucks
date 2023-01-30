using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.BackgroundJobs
{
    public class UpdateMileageJobArgs
    {
        public int TenantId { get; set; }
        public long UserId { get; set; }

    }
}
