using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class MoveTruckResult
    {
        public bool Success => !OrderLineTruckExists;
        public bool OrderLineTruckExists { get; set; }
    }
}
