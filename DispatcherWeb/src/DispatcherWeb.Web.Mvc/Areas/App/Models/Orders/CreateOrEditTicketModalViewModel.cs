﻿using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Web.Areas.App.Models.Orders
{
    public class CreateOrEditTicketModalViewModel
    {
        public int OrderLineId { get; set; }

        public DesignationEnum OrderLineDesignation { get; set; }

        public int Id { get; set; }

        [StringLength(Ticket.MaxTicketNumberLength)]
        [Required]
        public string TicketNumber { get; set; }

        public decimal MaterialQuantity { get; set; }

        public decimal FreightQuantity { get; set; }

        [Required]
        public int TruckId { get; set; }

        public int? ReceiptLineId { get; set; }

        public bool OrderLineIsProductionPay { get; internal set; }
    }
}
