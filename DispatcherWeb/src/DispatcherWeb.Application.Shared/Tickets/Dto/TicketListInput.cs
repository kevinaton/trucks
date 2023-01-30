using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? DateRangeBegin { get; set; }
        public DateTime? DateRangeEnd { get; set; }
        public DateTime? OrderDateRangeBegin { get; set; }
        public DateTime? OrderDateRangeEnd { get; set; }
        public int? OfficeId { get; set; }
        public int? InvoiceId { get; set; }
        public int? CarrierId { get; set; }
        public int? ServiceId { get; set; }
        public string TicketNumber { get; set; }
        public string TruckCode { get; set; }
        public Shift[] Shifts { get; set; }
        public bool? BillingStatus { get; set; }
        public bool? IsVerified { get; set; }
        public string CustomerName { get; set; }
        public string LoadAt { get; set; }
        public string DeliverTo { get; set; }
        public string JobNumber { get; set; }
        public TicketListStatusFilterEnum? TicketStatus { get; set; }
        public int[] TicketIds { get; set; }
        public int? DriverId { get; set; }
        public int? OrderId { get; set; }
        public bool? IsImported { get; set; }

        public void Normalize()
        {
            if(Sorting.IsNullOrEmpty())
            {
                Sorting = "Date";
            }

        }

    }
}
