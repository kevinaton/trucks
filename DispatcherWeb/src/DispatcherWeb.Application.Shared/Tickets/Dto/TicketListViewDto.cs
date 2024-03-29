﻿using System;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Orders.TaxDetails;
using Newtonsoft.Json;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketListViewDto : EditTicketFromListInput, IOrderLineTaxTotalDetails, ITicketQuantity
    {
        public DateTime? Date { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Office { get; set; }
        public string CustomerName { get; set; }
        public string Product { get; set; }
        public string TicketNumber { get; set; }
        public decimal Quantity { get; set; }
        public string Uom { get; set; }
        public string Carrier { get; set; }
        public string Truck { get; set; }
        public string TruckOffice { get; set; }
        public string Trailer { get; set; }
        public string DriverName { get; set; }
        public string DriverOffice { get; set; }
        public string JobNumber { get; set; }
        public string QuoteName { get; set; }
        public bool IsBilled { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public int? ReceiptLineId { get; set; }
        public int? InvoiceLineId { get; set; }
        public bool HasPayStatements { get; set; }
        public bool HasLeaseHaulerStatements { get; set; }

        public Shift? ShiftRaw { get; set; }
        public string Shift { get; set; }
        public string LoadAtNamePlain { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToNamePlain { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }

        public DesignationEnum? Designation { get; set; }

        DesignationEnum ITicketQuantity.Designation => Designation ?? 0;

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public int? TicketUomId { get; set; }
        public bool IsImported { get; set; }
        public decimal Rate => Quantity != 0 ? Math.Round(Subtotal / Quantity, 2) : 0;
        public decimal Revenue { get; set; }
        public bool? ProductionPay { get; set; }
        public int? PayStatementId { get; set; }

        public decimal? MaterialRate { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? MaterialAmount => Math.Round(this.GetMaterialQuantity() * MaterialRate ?? 0, 2);
        public decimal? FreightAmount => Math.Round(this.GetFreightQuantity() * FreightRate ?? 0, 2);
        public bool? IsFreightPriceOverridden { get; set; }
        public bool? IsMaterialPriceOverridden { get; set; }
        public decimal? OrderLineFreightPrice { get; set; }
        public decimal? OrderLineMaterialPrice { get; set; }
        public decimal? FuelSurcharge { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }
        public decimal FreightTotal => IsFreightPriceOverridden == true ? (OrderLineFreightPrice ?? 0) : Math.Round(this.GetFreightQuantity() * FreightRate ?? 0, 2);
        public decimal MaterialTotal => IsMaterialPriceOverridden == true ? (OrderLineMaterialPrice ?? 0) : Math.Round(this.GetMaterialQuantity() * MaterialRate ?? 0, 2);
        public decimal? PriceOverride
        {
            get
            {
                if (IsFreightPriceOverridden != true && IsMaterialPriceOverridden != true)
                {
                    return null;
                }

                decimal result = 0;
                if (IsFreightPriceOverridden == true && this.GetAmountTypeToUse().useFreight)
                {
                    result += OrderLineFreightPrice ?? 0;
                }

                if (IsMaterialPriceOverridden == true && this.GetAmountTypeToUse().useMaterial)
                {
                    result += OrderLineMaterialPrice ?? 0;
                }

                return result;
            }
        }

        public decimal Tax { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; } //=> MaterialTotal + FreightTotal + Tax;
        decimal IOrderLineTaxTotalDetails.TotalAmount { get => Total; set => Total = value; }
        public decimal? SalesTaxRate { get; set; }

        public bool? IsTaxable { get; set; }
        bool IOrderLineTaxDetails.IsTaxable => IsTaxable ?? false;
        decimal IOrderLineTaxDetails.MaterialPrice => MaterialTotal;
        decimal IOrderLineTaxDetails.FreightPrice => FreightTotal;
    }
}
