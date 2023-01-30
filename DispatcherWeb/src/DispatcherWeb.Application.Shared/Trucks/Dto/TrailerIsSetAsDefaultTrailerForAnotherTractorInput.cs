using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Trucks.Dto
{
    public class TrailerIsSetAsDefaultTrailerForAnotherTractorInput
    {
        public int TrailerId { get; set; }
        public int? TractorId { get; set; }
    }
}
