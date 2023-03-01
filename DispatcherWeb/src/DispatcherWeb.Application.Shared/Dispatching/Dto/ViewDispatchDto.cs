using System;
using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dispatching.Dto
{
    public class ViewDispatchDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public string CustomerName { get; set; }
        public string Item { get; set; }
        public decimal? MaterialQuantity => GetTicketToUse()?.GetMaterialQuantity();
        public decimal? FreightQuantity => GetTicketToUse()?.GetFreightQuantity();
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public DispatchStatus Status { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Loaded { get; set; }
        public DateTime? Delivered { get; set; }
        public List<ViewDispatchTicketDto> Tickets { get; set; }

        private ViewDispatchTicketDto GetTicketToUse()
        {
            if (Tickets?.Any() != true)
            {
                return null;
            }

            foreach (var ticket in Tickets)
            {
                var (useMaterial, useFreight) = ticket.GetAmountTypeToUse();
                if (useFreight)
                {
                    if (ticket.TicketUomId == FreightUomId)
                    {
                        return ticket;
                    }
                }
                else if (useMaterial)
                {
                    if (ticket.TicketUomId == MaterialUomId)
                    {
                        return ticket;
                    }
                }
            }

            return null;
        }
    }
}
