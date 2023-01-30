using DispatcherWeb.Orders.TaxDetails;
using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineDto : IOrderLineTaxDetails
    {
        public int Id { get; set; }

        public int LineNumber { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        //public decimal ActualQuantity { get; set; }

        public decimal? MaterialPricePerUnit { get; set; }

        public decimal? FreightPricePerUnit { get; set; }

        public bool IsMaterialPricePerUnitOverridden { get; set; }

        public bool IsFreightPricePerUnitOverridden { get; set; }

        public bool HasQuoteBasedPricing { get; set; }

        public string ServiceName { get; set; }

        public string LoadAtName { get; set; }

        public string DeliverToName { get; set; }

        public string MaterialUomName { get; set; }

        public string FreightUomName { get; set; }

        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();

        public decimal MaterialPrice { get; set; }

        public decimal FreightPrice { get; set; }

        public bool IsTaxable { get; set; }

        public string Note { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        //public ICollection<TicketDto> Tickets { get; set; }

        //public ICollection<OfficeAmountDto> OfficeAmounts { get; set; }

        //public ICollection<OrderLineShareDto> SharedOrderLines { get; set; }
    }
}
