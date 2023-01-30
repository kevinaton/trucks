using DispatcherWeb.Common.Dto;
using DispatcherWeb.Orders.TaxDetails;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Receipts.Dto
{
    public class ReceiptLineEditDto
    {
        public int? Id { get; set; }

        //public int TenantId { get; set; }

        public int ReceiptId { get; set; }

        public int? OrderLineId { get; set; }

        public int LineNumber { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; }


        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public string DesignationName => Designation.GetDisplayName();

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }


        public decimal MaterialRate { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal MaterialAmount { get; set; }


        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        public decimal FreightRate { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal FreightAmount { get; set; }

        public bool IsMaterialAmountOverridden { get; set; }

        public bool IsFreightAmountOverridden { get; set; }


        public bool IsMaterialRateOverridden { get; set; }

        public bool IsFreightRateOverridden { get; set; }

        public bool CanOverrideTotals => true;
        //public bool IsMultipleLoads { get; set; }
        public List<int> TicketIds { get; set; }
    }
}
