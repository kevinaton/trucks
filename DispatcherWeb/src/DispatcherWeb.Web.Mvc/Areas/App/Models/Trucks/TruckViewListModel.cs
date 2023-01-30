using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Trucks
{
    public class TruckViewListModel
    {
		public string OfficeName { get; set; }
		public GetTrucksInput Filter { get; set; }
        public bool IsGpsIntegrationConfigured { get; set; }
        public bool IsDtdTrackerConfigured { get; set; }
        public bool IsIntelliShiftConfigured { get; set; }
	}
}
