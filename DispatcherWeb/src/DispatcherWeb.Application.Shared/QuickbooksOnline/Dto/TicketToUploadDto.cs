﻿using System;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Tickets;
using Newtonsoft.Json;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class TicketToUploadDto : ITicketQuantity
    {
        public DateTime? TicketDateTimeUtc { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public string TicketUomName { get; set; }
        public bool? IsOrderLineMaterialTotalOverridden { get; set; }
        public bool? IsOrderLineFreightTotalOverridden { get; set; }
        public decimal? OrderLineMaterialTotal { get; set; }
        public decimal? OrderLineFreightTotal { get; set; }
        public DesignationEnum? Designation { get; set; }
        DesignationEnum ITicketQuantity.Designation => Designation ?? DesignationEnum.MaterialOnly;
        public string LoadAtName => LoadAt?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto LoadAt { get; set; }
        public string DeliverToName => DeliverTo?.FormattedAddress;
        [JsonIgnore]
        public LocationNameDto DeliverTo { get; set; }

        public bool HasOrderLine { get; set; }

        public decimal Quantity { get; set; }

        public TicketToUploadDto Clone()
        {
            return new TicketToUploadDto
            {
                TicketDateTimeUtc = TicketDateTimeUtc,
                FreightUomId = FreightUomId,
                MaterialUomId = MaterialUomId,
                TicketUomId = TicketUomId,
                TicketUomName = TicketUomName,
                IsOrderLineMaterialTotalOverridden = IsOrderLineMaterialTotalOverridden,
                IsOrderLineFreightTotalOverridden = IsOrderLineFreightTotalOverridden,
                OrderLineMaterialTotal = OrderLineMaterialTotal,
                OrderLineFreightTotal = OrderLineFreightTotal,
                Designation = Designation,
                HasOrderLine = HasOrderLine,
                Quantity = Quantity,
            };
        }
    }
}
