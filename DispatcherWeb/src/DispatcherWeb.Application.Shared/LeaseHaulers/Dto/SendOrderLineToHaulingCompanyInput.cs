using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class SendOrderLineToHaulingCompanyInput
    {
        public int OrderLineId { get; set; }

        [Required]
        public int? LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }
    }
}
