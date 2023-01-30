using Abp.Extensions;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Tickets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class CustomerTicketDto : IOrderLineTaxTotalDetails, ITicketQuantity
    {
        public int Id { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public bool IsTaxable { get; set; }
        public string TicketNumber { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
        public string TruckCode { get; set; }
        public string LoadAtNamePlain { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToNamePlain { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public decimal? MaterialRate { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightRate { get; set; }
        public DesignationEnum? Designation { get; set; }
        public string MaterialUomName { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public string FreightUomName { get; set; }
        public string JobNumber { get; set; }
        public string PoNumber { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal Tax { get; set; }
        public decimal? SalesTaxRate { get; set; }

        public bool? IsOrderLineFreightTotalOverridden { get; set; }

        public bool? IsOrderLineMaterialTotalOverridden { get; set; }

        public decimal? OrderLineFreightTotal { get; set; }

        public decimal? OrderLineMaterialTotal { get; set; }

        public decimal FreightTotal => IsOrderLineFreightTotalOverridden == true ? (OrderLineFreightTotal ?? 0) : Math.Round(this.GetFreightQuantity() * FreightRate ?? 0, 2);
        public decimal MaterialTotal => IsOrderLineMaterialTotalOverridden == true ? (OrderLineMaterialTotal ?? 0) : Math.Round(this.GetMaterialQuantity() * MaterialRate ?? 0, 2);

        decimal IOrderLineTaxDetails.MaterialPrice => MaterialTotal;

        decimal IOrderLineTaxDetails.FreightPrice => FreightTotal;
        //public InvoiceLineEditDto InvoiceLine { get; set; }
        public decimal Subtotal { get; set; }

        public decimal Total { get; set; } //=> MaterialTotal + FreightTotal + Tax;
        
        public string LeaseHaulerName { get; set; }

        public int? InvoiceLineId { get; set; }

        public InvoicingMethodEnum InvoicingMethod { get; set; }

        public string Description
        {
            get
            {
                var designationHasMaterial = Designation == DesignationEnum.MaterialOnly || Designation == DesignationEnum.FreightAndMaterial;
                var jobNumber = JobNumber.IsNullOrEmpty() ? "" : "; Job Nbr: " + JobNumber;
                var poNumber = PoNumber.IsNullOrEmpty() ? "" : "; PO Nbr: " + PoNumber;

                if (!designationHasMaterial && FreightUomName?.ToLower().StartsWith("hour") == true)
                {
                    return $"{Quantity} hours {ServiceName}{jobNumber}{poNumber}";
                }

                //if (designationHasMaterial)
                //{
                //    return $"{Quantity} {MaterialUomName} {ServiceName} from {LoadAt} to {DeliverTo}";
                //}

                var useFreight = this.GetAmountTypeToUse().useFreight;

                return $"{Quantity} {(useFreight ? FreightUomName : MaterialUomName)} {ServiceName} from {LoadAtName} to {DeliverToName}{jobNumber}{poNumber}";
            }
        }

        decimal IOrderLineTaxTotalDetails.TotalAmount { get => Total; set => Total = value; }

        DesignationEnum ITicketQuantity.Designation => Designation ?? 0;
    }
}
