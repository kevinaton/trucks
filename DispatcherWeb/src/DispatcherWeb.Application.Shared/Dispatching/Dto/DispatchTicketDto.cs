using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchTicketDto
    {
		public Guid Guid { get; set; }
        public int? LoadId { get; set; }
        public bool IsEdit { get; set; }
        public DispatchStatus? DispatchStatus { get; set; }
        public int? TimeClassificationId { get; set; }

        public string TicketNumber { get; set; }
        public decimal? Amount { get; set; }
		public decimal? MaterialAmount { get; set; }
        public decimal? FreightAmount { get; set; }

		public double? SourceLatitude { get; set; }
		public double? SourceLongitude { get; set; }

        public Guid? TicketPhotoId { get; set; }
        public Guid? DeferredPhotoId { get; set; }
        public string TicketPhotoFilename { get; set; }
        public string TicketPhotoBase64 { get; set; }
        public bool CreateNewTicket { get; set; }
        public bool TicketControlsWereHidden { get; set; }

    }
}
