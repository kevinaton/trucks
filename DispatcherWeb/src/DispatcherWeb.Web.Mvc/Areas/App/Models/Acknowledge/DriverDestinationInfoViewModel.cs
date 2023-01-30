using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Acknowledge
{
    public class DriverDestinationInfoViewModel : DriverInfoBaseViewModel
    {
		public Shift? Shift { get; set; }
		public DateTime Date { get; set; }
		public string CustomerName { get; set; }
		public string CustomerAddress { get; set; }
        public string Note { get; set; }
        public bool IsMultipleLoads { get; set; }
        public Guid? SignatureId { get; set; }


        public static DriverDestinationInfoViewModel CreateFrom(DriverDestinationInfoDto driverDestinationInfoDto)
		{
			return new DriverDestinationInfoViewModel()
			{
                DispatchId = driverDestinationInfoDto.DispatchId,
                TenantId = driverDestinationInfoDto.TenantId,
                Guid = driverDestinationInfoDto.Guid,
				CustomerName = driverDestinationInfoDto.CustomerName,
				CustomerAddress = driverDestinationInfoDto.CustomerAddress,
				Date = driverDestinationInfoDto.Date,
				Shift = driverDestinationInfoDto.Shift,
                Note = driverDestinationInfoDto.Note,
                IsMultipleLoads = driverDestinationInfoDto.IsMultipleLoads,
                SignatureId = driverDestinationInfoDto.SignatureId
			};
		}

	}
}
