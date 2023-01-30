using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTruckUtilizationDataInput
    {
        public TruckUtilizationDatePeriod DatePeriod { get; set; }
        public DateTime PeriodBegin { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
