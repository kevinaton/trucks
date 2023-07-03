using System;
using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Orders.TaxDetails;
using Newtonsoft.Json;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderSummaryReportItemDto
    {
        public int OrderId { get; set; }
        public int OrderLineId { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }
        public string CustomerName { get; set; }
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }
        public ItemOrderLine Item { get; set; }
        public decimal SalesTaxRate { get; set; }
        public double? NumberOfTrucks { get; set; }
        public string TrucksString => string.Join("\n", Trucks.Select(x => x.ToString()).OrderBy(x => x).Distinct().ToList());
        public List<ItemOrderLineTruck> Trucks { get; set; }
        public Shift? OrderShift { get; set; }
        public string OrderShiftName { get; set; }

        public class ItemOrderLine : IOrderLineTaxTotalDetails
        {
            public string ServiceName { get; set; }
            public bool IsTaxable { get; set; }
            public decimal? MaterialQuantity { get; set; }
            public decimal? FreightQuantity { get; set; }
            public double NumberOfTrucks { get; set; }
            public DesignationEnum Designation { get; set; }
            public string MaterialUom { get; set; }
            public string FreightUom { get; set; }
            public decimal MaterialPrice { get; set; }
            public decimal FreightPrice { get; set; }
            public decimal OrderLineTax { get; set; }
            public decimal OrderLineTotal { get; set; }

            decimal IOrderLineTaxTotalDetails.Subtotal { get; set; }
            decimal IOrderLineTaxTotalDetails.Tax { get => OrderLineTax; set => OrderLineTax = value; }
            decimal IOrderLineTaxTotalDetails.TotalAmount { get => OrderLineTotal; set => OrderLineTotal = value; }

            public string GetDisplayValue()
            {
                if (Designation.MaterialOnly())
                {
                    return $"{ServiceName}   {MaterialQuantity:f2} {MaterialUom}";
                }
                if (Designation == DesignationEnum.FreightOnly)
                {
                    return $"{ServiceName}   {FreightQuantity:f2} {FreightUom}";
                }
                var hasMaterial = MaterialQuantity > 0;
                var hasFreight = FreightQuantity > 0;

                var result = $"{ServiceName}";
                if (hasMaterial || hasFreight)
                {
                    result += "   ";
                }
                if (hasMaterial)
                {
                    result += $"{MaterialQuantity:f2} {MaterialUom}";
                }
                if (hasMaterial && hasFreight)
                {
                    result += " | ";
                }
                if (hasFreight)
                {
                    result += $"{FreightQuantity:f2} {FreightUom}";
                }

                return result;
            }
        }

        public class ItemOrderLineTruck
        {
            public string TruckCode { get; set; }
            public string TrailerTruckCode { get; set; }
            public string DriverName { get; set; }

            public override string ToString()
            {
                var result = TruckCode;
                if (!string.IsNullOrEmpty(TrailerTruckCode))
                {
                    result += " :: " + TrailerTruckCode;
                }

                if (!string.IsNullOrEmpty(DriverName))
                {
                    result += $" - ({DriverName})";
                }

                return result;
            }
        }
    }
}
