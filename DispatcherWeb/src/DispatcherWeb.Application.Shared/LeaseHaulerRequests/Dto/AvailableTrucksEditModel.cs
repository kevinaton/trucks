using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class AvailableTrucksEditModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Number of trucks available is required")]
        public int Available { get; set; }

        public List<int?> Trucks { get; set; }

        public List<int?> Drivers { get; set; }

        [StringLength(2000)]
        public string Comments { get; set; }

        // #8604 Commented until further notice
        //public int? Approved { get; set; }
        //public bool HasAvailableBeenSent { get; set; }
    }
}
