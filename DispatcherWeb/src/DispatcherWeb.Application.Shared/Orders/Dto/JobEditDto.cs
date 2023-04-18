using DispatcherWeb.Common.Dto;
using DispatcherWeb.Infrastructure;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Orders.Dto
{
    public class JobEditDto
    {
        
        public int? OrderId { get; set; }

        public int? OrderLineId { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public OrderPriority Priority { get; set; }


        [StringLength(EntityStringFieldLengths.Order.ChargeTo)]
        public string ChargeTo { get; set; }

        [StringLength(EntityStringFieldLengths.Order.PoNumber)]
        public string PONumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.SpectrumNumber)]
        public string SpectrumNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.Directions)]
        public string Directions { get; set; }

        public int? QuoteId { get; set; }

        public string QuoteName { get; set; }

        public Shift? Shift { get; set; }

        public int OfficeId { get; set; }

        public int? ProjectId { get; set; }

        public int? ContactId { get; set; }


        //public int LineNumber { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialPricePerUnit { get; set; }

        public decimal? FreightPricePerUnit { get; set; }

        public bool IsMaterialPricePerUnitOverridden { get; set; }

        public bool IsFreightPricePerUnitOverridden { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }
        public decimal? LeaseHaulerRate { get; set; }

        [Required(ErrorMessage = "Service/Product Item is a required field")]
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public bool IsTaxable { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        
        [JsonIgnore]
        public string LoadAtNamePlain { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        
        [JsonIgnore]
        public string DeliverToNamePlain { get; set; }

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public string DesignationName => Designation.GetDisplayName();

        public decimal MaterialPrice { get; set; }
        
        public decimal FreightPrice { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public double? NumberOfTrucks { get; set; }

        public decimal SalesTaxRate { get; set; }

        public decimal SalesTax { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public StaggeredTimeKind StaggeredTimeKind { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        public bool CanOverrideTotals { get; set; }

        public bool IsMultipleLoads { get; set; }

        public bool ProductionPay { get; set; }

        public bool AutoGenerateTicketNumber { get; set; }

        public int? TicketId { get; set; }

        [StringLength(EntityStringFieldLengths.Ticket.TicketNumber)]
        public string TicketNumber { get; set; }

        public bool HasQuoteBasedPricing { get; set; }
        public bool HasTickets { get; set; }
        public bool HasOpenDispatches { get; set; }
        public bool IsTimeStaggeredForTrucks { get; set; }
        public DateTime? FirstStaggeredTimeOnJob { get; set; }
        public int? StaggeredTimeInterval { get; set; }
        public bool UpdateStaggeredTime { get; set; }
        public int? QuoteServiceId { get; set; }
        public string FocusFieldId { get; set; }
        public int? MaterialCompanyOrderId { get; set; }
        public int? DefaultLoadAtLocationId { get; set; }
        public string DefaultLoadAtLocationName { get; set; }
        public int? DefaultServiceId { get; set; }
        public string DefaultServiceName { get; set; }
        public int? DefaultMaterialUomId { get; set; }
        public string DefaultMaterialUomName { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public string FuelSurchargeCalculationName { get; set; }

        public bool? CanChangeBaseFuelCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? BaseFuelCost { get; set; }

        public string DefaultFuelSurchargeCalculationName { get; set; }

        public decimal? DefaultBaseFuelCost { get; set; }

        public bool? DefaultCanChangeBaseFuelCost { get; set; }

        public bool RequiresCustomerNotification { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationContactName)]
        public string CustomerNotificationContactName { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationPhoneNumber)]
        public string CustomerNotificationPhoneNumber { get; set; }
    }
}
