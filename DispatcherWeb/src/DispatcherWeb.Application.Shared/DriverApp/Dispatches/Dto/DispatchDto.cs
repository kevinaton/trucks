using System;
using System.Collections.Generic;
using DispatcherWeb.DriverApp.Loads.Dto;
using DispatcherWeb.DriverApp.Locations.Dto;
using DispatcherWeb.DriverApp.Tickets.Dto;

namespace DispatcherWeb.DriverApp.Dispatches.Dto
{
    public class DispatchDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string CustomerName { get; set; }
        public CustomerContactDto CustomerContact { get; set; }
        public DateTime OrderDate { get; set; }
        public Shift? Shift { get; set; }
        public DispatchStatus Status { get; set; }
        public DesignationEnum Designation { get; set; }
        public string Item { get; set; }
        public LocationDto LoadAt { get; set; }
        public LocationDto DeliverTo { get; set; }
        public CustomerNotificationDto CustomerNotification { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public bool IsCOD { get; set; }
        public string ChargeTo { get; set; }
        public string MaterialUOM { get; set; }
        public string FreightUOM { get; set; }
        public DateTime LastModifiedDateTime { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public bool ProductionPay { get; set; }
        public bool IsMultipleLoads { get; set; }
        public List<LoadDto> Loads { get; set; }
        public List<TicketDto> Tickets { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public DateTime? AcknowledgedDateTime { get; set; }
        public int? OrderLineTruckId { get; set; }
        public int SortOrder { get; set; }

        public string QuantityWithItem
        {
            get
            {
                var material = $"{MaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {MaterialUOM} {Item}";
                var freight = $"{FreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {FreightUOM} {Item}";

                if (Designation.MaterialOnly())
                {
                    return material;
                }

                if (Designation == DesignationEnum.FreightAndMaterial)
                {
                    if (MaterialUOM == FreightUOM)
                    {
                        return material;
                    }

                    return material + Environment.NewLine + freight;
                }

                return freight;
            }
        }
    }
}
