using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Web.Areas.App.Models.Acknowledge
{
    public class DriverLoadInfoViewModel : DriverInfoBaseViewModel
    {
        public int? LoadId { get; set; }
        public Shift? Shift { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        public DispatchStatus DispatchStatus { get; set; }
        public string Item { get; set; }
        public string PickupAt { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public string Note { get; set; }
        public string ChargeTo { get; set; }
        public bool IsMultipleLoads { get; set; }

        [StringLength(Ticket.MaxTicketNumberLength)]
        public string TicketNumber { get; set; }
        public bool CreateNewTicket { get; set; }
        public decimal Amount { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }


        public static DriverLoadInfoViewModel CreateFrom(DriverLoadInfoDto driverLoadInfoDto)
        {
            return new DriverLoadInfoViewModel()
            {
                DispatchId = driverLoadInfoDto.DispatchId,
                TenantId = driverLoadInfoDto.TenantId,
                Guid = driverLoadInfoDto.Guid,
                LoadId = driverLoadInfoDto.LoadId,
                CustomerName = driverLoadInfoDto.CustomerName,
                Date = driverLoadInfoDto.Date,
                Shift = driverLoadInfoDto.Shift,
                DispatchStatus = driverLoadInfoDto.DispatchStatus,
                Item = driverLoadInfoDto.Item,
                Designation = driverLoadInfoDto.Designation,
                PickupAt = driverLoadInfoDto.PickupAt,
                MaterialQuantity = driverLoadInfoDto.MaterialQuantity,
                FreightQuantity = driverLoadInfoDto.FreightQuantity,
                Note = driverLoadInfoDto.Note,
                ChargeTo = driverLoadInfoDto.ChargeTo,
                IsMultipleLoads = driverLoadInfoDto.IsMultipleLoads,

                TicketNumber = driverLoadInfoDto.TicketNumber,
                Amount = driverLoadInfoDto.Amount,
                MaterialUomName = driverLoadInfoDto.MaterialUomName,
                FreightUomName = driverLoadInfoDto.FreightUomName
            };
        }

    }
}
